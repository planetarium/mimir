using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mimir.GraphQL;
using Mimir.Options;
using Mimir.Repositories;
using Mimir.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<HeadlessStateServiceOption>(
    builder.Configuration.GetRequiredSection(HeadlessStateServiceOption.SectionName)
);
builder.Services.Configure<DatabaseOption>(
    builder.Configuration.GetRequiredSection(DatabaseOption.SectionName)
);
builder.Services.Configure<RateLimitOption>(
    builder.Configuration.GetRequiredSection(RateLimitOption.SectionName)
);
builder.Services.Configure<JwtOption>(
    builder.Configuration.GetRequiredSection(JwtOption.SectionName)
);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(
        JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            var jwtOptions = builder
                .Configuration.GetRequiredSection(JwtOption.SectionName)
                .Get<JwtOption>();

            if (jwtOptions is null)
            {
                throw new InvalidOperationException("JWT options are not configured properly.");
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
            };
        }
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IStateService, HeadlessStateService>();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<TableSheetsRepository>();
builder.Services.AddSingleton<MetadataRepository>();
builder.Services.AddSingleton<AgentRepository>();
// AvatarState dependencies
builder.Services.AddSingleton<AvatarRepository>();
builder.Services.AddSingleton<ActionPointRepository>();
builder.Services.AddSingleton<DailyRewardRepository>();

// builder.Services.AddSingleton<InventoryRepository>();
// builder.Services.AddSingleton<AllRuneRepository>();
// builder.Services.AddSingleton<CollectionRepository>();
builder.Services.AddSingleton<ItemSlotRepository>();
builder.Services.AddSingleton<RuneSlotRepository>();
builder.Services.AddSingleton<ArenaRepository>();
// builder.Services.AddSingleton<StakeRepository>();
// builder.Services.AddSingleton<ProductRepository>();
// builder.Services.AddSingleton<SeasonInfoRepository>();
builder.Services.AddControllers();
builder
    .Services.AddHeadlessGQLClient()
    .ConfigureHttpClient(
        (provider, client) =>
        {
            var headlessStateServiceOption = provider.GetRequiredService<
                IOptions<HeadlessStateServiceOption>
            >();
            var option = headlessStateServiceOption.Value;
            client.BaseAddress = new Uri(option.Endpoint);

            if (option.JwtSecretKey is not null && option.JwtIssuer is not null)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.JwtSecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: option.JwtIssuer,
                    expires: DateTime.UtcNow.AddMinutes(5),
                    signingCredentials: creds
                );

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    new JwtSecurityTokenHandler().WriteToken(token)
                );
            }
        }
    );
builder.Services.AddCors();
builder.Services.AddHttpClient();
builder
    .Services.AddGraphQLServer()
    .AddLib9cGraphQLTypes()
    .AddMimirGraphQLTypes()
    .BindRuntimeType(typeof(Address), typeof(AddressType))
    .AddErrorFilter<ErrorFilter>()
    .ModifyRequestOptions(requestExecutorOptions =>
    {
        requestExecutorOptions.IncludeExceptionDetails = true;
    });

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    limiterOptions.AddPolicy(
        policyName: "jwt",
        partitioner: httpContext =>
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
            var rateLimitOptions = httpContext
                .RequestServices.GetRequiredService<IOptions<RateLimitOption>>()
                .Value;

            if (isAuthenticated)
            {
                return RateLimitPartition.GetNoLimiter("NoLimit");
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetTokenBucketLimiter(
                ipAddress,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = rateLimitOptions.TokenLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitOptions.QueueLimit,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(
                        rateLimitOptions.ReplenishmentPeriod
                    ),
                    TokensPerPeriod = rateLimitOptions.TokensPerPeriod,
                    AutoReplenishment = rateLimitOptions.AutoReplenishment
                }
            );
        }
    );
});

var app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Health Check").RequireRateLimiting("jwt");
app.MapGraphQL().RequireRateLimiting("jwt");

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors(policy =>
{
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
});

app.Run();
