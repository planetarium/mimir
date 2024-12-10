using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mimir.Initializer;
using Mimir.Initializer.Initializer;
using Mimir.Initializer.Migrators;
using Mimir.Worker.Services;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (hostingContext, config) =>
        {
            string configPath =
                Environment.GetEnvironmentVariable("INITIALIZER_CONFIG_FILE") ?? "appsettings.json";
            config
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("INITIALIZER_");
        }
    )
    .ConfigureServices(
        (hostContext, services) =>
        {
            services.Configure<Configuration>(
                hostContext.Configuration.GetSection("Configuration")
            );

            services.AddSingleton<MongoDbService>();

            services.AddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
                return new MongoDbService(
                    config.MongoDbConnectionString,
                    config.PlanetType,
                    config.MongoDbCAFile
                );
            });

            services.AddTransient<SnapshotInitializer>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
                var dbService = serviceProvider.GetRequiredService<MongoDbService>();
                var targetAccounts = config.GetTargetAddresses();
                return new SnapshotInitializer(dbService, config.ChainStorePath, targetAccounts);
            });
            
            services.AddSingleton<IItemProductCalculationService, ItemProductCalculationService>();
            services.AddSingleton<ProductMigrator>();
        }
    )
    .UseSerilog(
        (context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        }
    )
    .Build();

var config = host.Services.GetRequiredService<IOptions<Configuration>>().Value;

if(config.RunOptions.HasFlag(RunOptions.SnapShotInitializer))
{
    await host.RunSnapShotInitializerAsync();
}
if(config.RunOptions.HasFlag(RunOptions.ProductMigrator))
{
    await host.RunProductMigratorAsync();
}
