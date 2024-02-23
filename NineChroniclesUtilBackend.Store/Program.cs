using NineChroniclesUtilBackend.Store;
using NineChroniclesUtilBackend.Store.Client;
using NineChroniclesUtilBackend.Store.Services;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

string configPath = Environment.GetEnvironmentVariable("STORE_CONFIG_FILE") ?? "appsettings.json";
builder.Configuration
    .AddJsonFile(configPath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("STORE_");

builder.Services.Configure<Configuration>(builder.Configuration.GetSection("Configuration"));

builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    return new EmptyChroniclesClient(config.EmptyChronicleBaseUrl);
});

builder.Services.AddSingleton<IStateService, EmptyChronicleStateService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
