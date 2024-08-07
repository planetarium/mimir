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
    private readonly MongoDbService _dbService;
    private readonly IStateService _stateService;
    private readonly HeadlessGQLClient _gqlClient;
    private readonly IBlockPoller _poller;
    private readonly bool _enableInitializing;

    public Worker(
        HeadlessGQLClient gqlClient,
        IStateService stateService,
        MongoDbService dbService,
        PollerType pollerType,
        bool enableInitializing
    )
    {
        _stateService = stateService;
        _dbService = dbService;
        _gqlClient = gqlClient;
        _enableInitializing = enableInitializing;

        _poller = PollerFactory.CreatePoller(pollerType, stateService, gqlClient, dbService);

        _logger = Log.ForContext<Worker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.Information(
            "Start Worker background service, init: {EnableInitializing}",
            _enableInitializing
        );

        if (_enableInitializing)
        {
            _logger.Information("Initializing enabled, start initializing");

            var initializerManager = new InitializerManager(_stateService, _dbService);
            await initializerManager.RunInitializersAsync(stoppingToken);
        }

        try
        {
            _logger.Information("Start Polling");
            await _poller.RunAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "An error occurred during polling.");
            throw;
        }

        _logger.Information(
            "Stopped Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
        await StopAsync(stoppingToken);
    }
}
