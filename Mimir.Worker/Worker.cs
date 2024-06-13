using HeadlessGQL;
using Mimir.Worker.Initializer;
using Mimir.Worker.Poller;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly MongoDbService _store;
    private readonly IStateService _stateService;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly string _snapshotPath;
    private readonly bool _enableSnapshotInitializing;
    private readonly bool _enableInitializing;

    public Worker(
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbService store,
        string snapshotPath,
        bool enableSnapshotInitializing,
        bool enableInitializing
    )
    {
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
        _snapshotPath = snapshotPath;
        _enableSnapshotInitializing = enableSnapshotInitializing;
        _enableInitializing = enableInitializing;

        _logger = Log.ForContext<Worker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.Information(
            "Start Worker background service. snapshot-init: {EnableSnapshotInitializing}, init: {EnableInitializing}",
            _enableSnapshotInitializing,
            _enableInitializing
        );

        var diffPoller = new DiffBlockPoller(_stateService, _headlessGqlClient, _store);
        var blockPoller = new BlockPoller(_stateService, _headlessGqlClient, _store);

        if (_enableSnapshotInitializing)
        {
            _logger.Information("Snapshot Initializing enabled, start initializing");

            var initializer = new SnapshotInitializer(_store, _snapshotPath);
            await initializer.RunAsync(stoppingToken);
        }

        if (_enableInitializing)
        {
            _logger.Information("Initializing enabled, start initializing");

            var initializerManager = new InitializerManager(_stateService, _store);
            await initializerManager.RunInitializersAsync(stoppingToken);
        }

        _logger.Information("Start Polling");
        await Task.WhenAll(diffPoller.RunAsync(stoppingToken), blockPoller.RunAsync(stoppingToken));

        _logger.Information(
            "Stopped Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
