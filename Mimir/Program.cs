using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Redis;
using Lib9c.GraphQL.Types;
using Lib9c.Models.Block;
using Libplanet.Common;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mimir.GraphQL;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.MongoDB.Services;
using Mimir.Options;
using Mimir.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Options;
using Mimir.Shared.Services;
using StackExchange.Redis;
using BalanceRepository = Mimir.MongoDB.Repositories.BalanceRepository;

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
builder.Services.Configure<WncgApiOption>(
    builder.Configuration.GetRequiredSection(WncgApiOption.SectionName)
);
builder.Services.Configure<HangfireOption>(
    builder.Configuration.GetRequiredSection(HangfireOption.SectionName)
);
builder.Services.Configure<HeadlessOption>(
    builder.Configuration.GetRequiredSection(HeadlessOption.SectionName)
);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

builder.WebHost.UseSentry();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IHeadlessGQLClient, HeadlessGQLClient>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<HeadlessOption>>().Value;
    return new HeadlessGQLClient(config.HeadlessEndpoints, config.JwtIssuer, config.JwtSecretKey);
});
builder.Services.AddSingleton<IStateService, HeadlessStateService>();
builder.Services.AddSingleton<IStateGetterService, StateGetterService>(serviceProvider =>
{
    var stateService = serviceProvider.GetRequiredService<IStateService>();
    var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseOption>>().Value;
    return new StateGetterService(stateService, dbConfig.PlanetType);
});

builder.Services.AddSingleton<IMongoDbService, MongoDbService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<DatabaseOption>>().Value;

    return new MongoDbService(
        config.ConnectionString,
        config.PlanetType.ToString().ToLowerInvariant(),
        config.CAFile
    );
});

// NOTE: MongoDB repositories. Sort in alphabetical order.
builder.Services.AddSingleton<IActionPointRepository, ActionPointRepository>();
builder.Services.AddSingleton<IActionTypeRepository, ActionTypeRepository>();
builder.Services.AddSingleton<IAgentRepository, AgentRepository>();
builder.Services.AddSingleton<
    IAllCombinationSlotStateRepository,
    AllCombinationSlotStateRepository
>();
builder.Services.AddSingleton<AllRuneRepository>();
builder.Services.AddSingleton<IAvatarRepository, AvatarRepository>();
builder.Services.AddSingleton<IBalanceRepository, BalanceRepository>();
builder.Services.AddSingleton<ICollectionRepository, CollectionRepository>();
builder.Services.AddSingleton<IDailyRewardRepository, DailyRewardRepository>();
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IPetRepository, PetRepository>();
builder.Services.AddSingleton<IMetadataRepository, MetadataRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IItemSlotRepository, ItemSlotRepository>();
builder.Services.AddSingleton<IPledgeRepository, PledgeRepository>();
builder.Services.AddSingleton<IProductsRepository, ProductsRepository>();
builder.Services.AddSingleton<IRuneSlotRepository, RuneSlotRepository>();
builder.Services.AddSingleton<IStakeRepository, StakeRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<TableSheetsRepository>();
builder.Services.AddSingleton<WorldBossKillRewardRecordRepository>();
builder.Services.AddSingleton<WorldBossRaiderRepository>();
builder.Services.AddSingleton<WorldBossRepository>();
builder.Services.AddSingleton<IWorldInformationRepository, WorldInformationRepository>();
builder.Services.AddSingleton<
    IWorldInformationRankingRepository,
    WorldInformationRankingRepository
>();
builder.Services.AddSingleton<
    ICpRepository<AdventureCpDocument>,
    CpRepository<AdventureCpDocument>
>();
builder.Services.AddSingleton<ICpRepository<ArenaCpDocument>, CpRepository<ArenaCpDocument>>();
builder.Services.AddSingleton<ICpRepository<RaidCpDocument>, CpRepository<RaidCpDocument>>();

// ~MongoDB repositories.

// Hangfire and Redis configuration
var hangfireOption = builder
    .Configuration.GetSection(HangfireOption.SectionName)
    .Get<HangfireOption>();
if (hangfireOption != null)
{
    var redisConfig = new ConfigurationOptions { DefaultDatabase = hangfireOption.RedisDatabase };

    redisConfig.EndPoints.Add(hangfireOption.RedisHost, hangfireOption.RedisPort);

    if (!string.IsNullOrEmpty(hangfireOption.RedisUsername))
    {
        redisConfig.User = hangfireOption.RedisUsername;
    }

    if (!string.IsNullOrEmpty(hangfireOption.RedisPassword))
    {
        redisConfig.Password = hangfireOption.RedisPassword;
    }

    var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

    builder.Services.AddHangfire(
        (provider, config) =>
        {
            config.UseRedisStorage(
                connectionMultiplexer,
                new RedisStorageOptions
                {
                    Prefix = hangfireOption.RedisPrefix,
                    Db = hangfireOption.RedisDatabase,
                }
            );
        }
    );

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = hangfireOption.WorkerCount;
    });
}

// State recovery service
builder.Services.AddScoped<IStateRecoveryService, StateRecoveryService>();
builder.Services.AddCors();
builder.Services.AddHttpClient();
builder
    .Services.AddGraphQLServer()
    .AllowIntrospection(true)
    .AddLib9cGraphQLTypes()
    .AddMimirGraphQLTypes()
    .AddErrorFilter<ErrorFilter>()
    .AddMongoDbPagingProviders(providerName: "MongoDB", defaultProvider: true)
    .SetPagingOptions(
        new HotChocolate.Types.Pagination.PagingOptions { MaxPageSize = 300, DefaultPageSize = 100 }
    )
    .BindRuntimeType(typeof(Address), typeof(AddressType))
    .BindRuntimeType(typeof(BigInteger), typeof(BigIntegerType))
    .BindRuntimeType(typeof(HashDigest<SHA256>), typeof(HashDigestSHA256Type))
    .BindRuntimeType(typeof(Lib9c.Models.Block.Action), typeof(Lib9c.GraphQL.Types.ActionType))
    .BindRuntimeType(typeof(Lib9c.Models.Block.Transaction), typeof(TransactionType))
    .BindRuntimeType(typeof(MongoDB.Bson.BsonDocument), typeof(BsonDocumentType))
    .ModifyRequestOptions(requestExecutorOptions =>
    {
        requestExecutorOptions.IncludeExceptionDetails = true;
    })
    .ModifyCostOptions(options =>
    {
        options.MaxFieldCost = 3_000;
        options.MaxTypeCost = 3_000;
    });
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
                    AutoReplenishment = rateLimitOptions.AutoReplenishment,
                }
            );
        }
    );
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IWncgPriceService, WncgPriceService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

var app = builder.Build();

// Hangfire dashboard
if (hangfireOption != null)
{
    app.UseHangfireDashboard(
        hangfireOption.DashboardPath,
        new DashboardOptions
        {
            Authorization = new[]
            {
                new BasicAuthDashboardAuthorizationFilter(
                    app.Services.GetRequiredService<IOptions<HangfireOption>>()
                ),
            },
        }
    );
}

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
