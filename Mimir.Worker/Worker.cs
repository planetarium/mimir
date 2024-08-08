using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Initializer;
using Mimir.Worker.Poller;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly PollerType _pollerType;
    private readonly bool _enableInitializing;
    public IServiceProvider Services { get; }

    public Worker(IServiceProvider services, PollerType pollerType, bool enableInitializing)
    {
        Services = services;
        _pollerType = pollerType;
        _enableInitializing = enableInitializing;

        _logger = Log.ForContext<Worker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.Information(
            "Start Worker background service, init: {EnableInitializing}",
            _enableInitializing
        );

        await DoWork(stoppingToken);

        _logger.Information(
            "Stopped Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
        // Temp force exits
        throw new Exception();
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        using (var scope = Services.CreateScope())
        {
            var gqlClient = scope.ServiceProvider.GetRequiredService<IHeadlessGQLClient>();
            var stateService = scope.ServiceProvider.GetRequiredService<IStateService>();
            var dbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

            if (_enableInitializing)
            {
                _logger.Information("Initializing enabled, start initializing");

                var initializerManager = new InitializerManager(stateService, dbService);
                await initializerManager.RunInitializersAsync(stoppingToken);
            }

            try
            {
                _logger.Information("Start Polling");
                var poller = PollerFactory.CreatePoller(
                    _pollerType,
                    stateService,
                    gqlClient,
                    dbService
                );

                await poller.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "An error occurred during polling.");
                throw;
            }
        }
    }
}
