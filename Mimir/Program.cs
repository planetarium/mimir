using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Lib9c.GraphQL.Types;
using Libplanet.Common;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.GraphQL;
using Mimir.MongoDB.Repositories;
using Mimir.Options;
using Mimir.MongoDB.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

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

builder.WebHost.UseSentry();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();
// NOTE: MongoDB repositories. Sort in alphabetical order.
builder.Services.AddSingleton<IActionPointRepository, ActionPointRepository>();
builder.Services.AddSingleton<IAgentRepository, AgentRepository>();
builder.Services.AddSingleton<IAllCombinationSlotStateRepository, AllCombinationSlotStateRepository>();
builder.Services.AddSingleton<AllRuneRepository>();
builder.Services.AddSingleton<ArenaRepository>();
builder.Services.AddSingleton<ArenaParticipantRepository>();
builder.Services.AddSingleton<IAvatarRepository, AvatarRepository>();
builder.Services.AddSingleton<BalanceRepository>();
builder.Services.AddSingleton<CollectionRepository>();
builder.Services.AddSingleton<DailyRewardRepository>();
builder.Services.AddSingleton<InventoryRepository>();
builder.Services.AddSingleton<ItemSlotRepository>();
builder.Services.AddSingleton<MetadataRepository>();
builder.Services.AddSingleton<PetRepository>();
builder.Services.AddSingleton<PledgeRepository>();
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<ProductsRepository>();
builder.Services.AddSingleton<StakeRepository>();
builder.Services.AddSingleton<TableSheetsRepository>();
builder.Services.AddSingleton<WorldBossKillRewardRecordRepository>();
builder.Services.AddSingleton<WorldBossRaiderRepository>();
builder.Services.AddSingleton<WorldBossRepository>();
builder.Services.AddSingleton<WorldInformationRepository>();
// ~MongoDB repositories.
builder.Services.AddCors();
builder.Services.AddHttpClient();
builder.Services
    .AddGraphQLServer()
    .AddLib9cGraphQLTypes()
    .AddMimirGraphQLTypes()
    .AddErrorFilter<ErrorFilter>()
    .AddMongoDbPagingProviders(providerName: "MongoDB", defaultProvider: true)
    .BindRuntimeType(typeof(Address), typeof(AddressType))
    .BindRuntimeType(typeof(BigInteger), typeof(BigIntegerType))
    .BindRuntimeType(typeof(HashDigest<SHA256>), typeof(HashDigestSHA256Type))
    .ModifyRequestOptions(requestExecutorOptions => { requestExecutorOptions.IncludeExceptionDetails = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpResponseFormatter<HttpResponseFormatter>();

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
