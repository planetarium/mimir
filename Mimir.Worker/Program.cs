using Microsoft.Extensions.Options;
using Mimir.Worker;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

string configPath = Environment.GetEnvironmentVariable("WORKER_CONFIG_FILE") ?? "appsettings.json";
builder
    .Configuration.AddJsonFile(configPath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("WORKER_");

builder.Services.Configure<Configuration>(builder.Configuration.GetSection("Configuration"));

var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration);
if (builder.Configuration.GetSection("Configuration").GetValue<string>("SentryDsn") is { } sentryDsn)
{
    loggerConfiguration = loggerConfiguration.WriteTo.Sentry(sentryDsn);
}

Log.Logger = loggerConfiguration.CreateLogger();
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
        config.PlanetType,
        config.MongoDbCAFile
    );
});

builder.Services.AddSingleton<IItemProductCalculationService, ItemProductCalculationService>();

builder.ConfigureInitializers();
builder.ConfigureHandlers();

var host = builder.Build();
var config = host.Services.GetRequiredService<IOptions<Configuration>>().Value;

host.Run();
