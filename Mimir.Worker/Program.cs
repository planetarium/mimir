using Microsoft.Extensions.Options;
using Mimir.Worker;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

string configPath = Environment.GetEnvironmentVariable("WORKER_CONFIG_FILE") ?? "appsettings.json";
builder
    .Configuration.AddJsonFile(configPath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("WORKER_");

builder.Services.Configure<Configuration>(builder.Configuration.GetSection("Configuration"));

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddSingleton<IHeadlessGQLClient, HeadlessGQLClient>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    return new HeadlessGQLClient(config.HeadlessEndpoints, config.JwtIssuer, config.JwtSecretKey);
});
builder.Services.AddSingleton<IStateService, HeadlessStateService>();

builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    return new MongoDbService(
        config.MongoDbConnectionString,
        PlanetType.FromString(config.PlanetType),
        config.MongoDbCAFile
    );
});
builder.Services.AddHostedService(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    
    AddressHandlerMappings.RegisterCurrencyHandler(PlanetType.FromString(config.PlanetType));

    return new Worker(serviceProvider, config.PollerType, config.EnableInitializing);
});

var host = builder.Build();
var config = host.Services.GetRequiredService<IOptions<Configuration>>().Value;

host.Run();
