using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Mimir.Services;
using Mimir.Options;
using Mimir.Repositories;
using Lib9c.GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<HeadlessStateServiceOption>(builder.Configuration.GetRequiredSection("StateService"));
builder.Services.Configure<DatabaseOption>(builder.Configuration.GetRequiredSection("Database"));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IStateService, HeadlessStateService>();
builder.Services.AddSingleton<MongoDBCollectionService>();
builder.Services.AddSingleton<ArenaRankingRepository>();
builder.Services.AddSingleton<TableSheetsRepository>();
builder.Services.AddSingleton<MetadataRepository>();
builder.Services.AddSingleton<AgentRepository>();
builder.Services.AddSingleton<AvatarRepository>();
builder.Services.AddSingleton<ActionPointRepository>();
builder.Services.AddSingleton<DailyRewardRepository>();
builder.Services.AddSingleton<InventoryRepository>();
builder.Services.AddSingleton<AllRuneRepository>();
builder.Services.AddSingleton<CollectionRepository>();
builder.Services.AddSingleton<ProductsRepository>();
builder.Services.AddControllers();
builder.Services.AddHeadlessGQLClient()
    .ConfigureHttpClient((provider, client) =>
    {
        var headlessStateServiceOption = provider.GetRequiredService<IOptions<HeadlessStateServiceOption>>();
        var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext is null
            || !httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("network", out object? value)
            || value is not string network
            || !headlessStateServiceOption.Value.Endpoints.TryGetValue(network, out var endpoint)) return;
        client.BaseAddress = endpoint.Endpoint;

        if (endpoint.JwtSecretKey is not null && endpoint.JwtIssuer is not null)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(endpoint.JwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: endpoint.JwtIssuer,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        }
    });
builder.Services.AddCors();
builder.Services.AddHttpClient();
builder.Services
    .AddGraphQLServer()
    .AddFiltering()
    .AddSorting()
    .AddLib9cGraphQLTypes()
    .AddMimirGraphQLTypes();

var app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Health Check");
app.MapControllers();
app.MapGraphQL();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(policy =>
{
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
});

app.Run();
