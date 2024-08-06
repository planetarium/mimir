using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using HeadlessGQL;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddSingleton<IStateService, HeadlessStateService>();
builder
    .Services.AddHeadlessGQLClient()
    .ConfigureHttpClient(
        (provider, client) =>
        {
            var headlessStateServiceOption = provider.GetRequiredService<IOptions<Configuration>>();
            client.BaseAddress = headlessStateServiceOption.Value.HeadlessEndpoint;

            if (
                headlessStateServiceOption.Value.JwtSecretKey is not null
                && headlessStateServiceOption.Value.JwtIssuer is not null
            )
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(headlessStateServiceOption.Value.JwtSecretKey)
                );
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: headlessStateServiceOption.Value.JwtIssuer,
                    expires: DateTime.UtcNow.AddMinutes(5),
                    signingCredentials: creds
                );

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    new JwtSecurityTokenHandler().WriteToken(token)
                );
            }

            client.Timeout = TimeSpan.FromSeconds(60);
        }
    );

builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    return new MongoDbService(
        config.MongoDbConnectionString,
        config.DatabaseName,
        config.MongoDbCAFile
    );
});
builder.Services.AddHostedService(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    var headlessGqlClient = serviceProvider.GetRequiredService<HeadlessGQLClient>();
    var stateService = serviceProvider.GetRequiredService<IStateService>();
    var store = serviceProvider.GetRequiredService<MongoDbService>();

    return new Worker(
        headlessGqlClient,
        new TempGQLClient(config.HeadlessEndpoint, config.JwtIssuer, config.JwtSecretKey),
        stateService,
        store,
        config.ActivePollers,
        config.SnapshotPath,
        config.EnableSnapshotInitializing,
        config.EnableInitializing
    );
});

var host = builder.Build();
var config = host.Services.GetRequiredService<IOptions<Configuration>>().Value;

host.Run();
