using HeadlessGQL;
using Mimir.Worker.Initializer;
using Mimir.Worker.Poller;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly MongoDbService _store;
    private readonly ILogger<Worker> _logger;
    private readonly ILogger<SnapshotInitializer> _initializerLogger;
    private readonly ILogger<BlockPoller> _blockPollerLogger;
    private readonly ILogger<DiffBlockPoller> _diffBlockPollerLogger;
    private readonly IStateService _stateService;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly string _snapshotPath;
    private readonly bool _enableSnapshotInitializing;
    private readonly bool _enableInitializing;

    public Worker(
        ILogger<Worker> logger,
        ILogger<BlockPoller> blockPollerLogger,
        ILogger<DiffBlockPoller> diffBlockPollerLogger,
        ILogger<SnapshotInitializer> initializerLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbService store,
        string snapshotPath,
        bool enableSnapshotInitializing,
        bool enableInitializing
    )
    {
        _logger = logger;
        _initializerLogger = initializerLogger;
        _blockPollerLogger = blockPollerLogger;
        _diffBlockPollerLogger = diffBlockPollerLogger;
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
        _snapshotPath = snapshotPath;
        _enableSnapshotInitializing = enableSnapshotInitializing;
        _enableInitializing = enableInitializing;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        var diffPoller = new DiffBlockPoller(
            _diffBlockPollerLogger,
            _stateService,
            _headlessGqlClient,
            _store
        );
        var blockPoller = new BlockPoller(
            _blockPollerLogger,
            _stateService,
            _headlessGqlClient,
            _store
        );

        if (_enableSnapshotInitializing)
        {
            var initializer = new SnapshotInitializer(_initializerLogger, _store, _snapshotPath);
            await initializer.RunAsync(stoppingToken);
        }

        if (_enableInitializing)
        {
            var initializerManager = new InitializerManager(_stateService, _store);
            await initializerManager.RunInitializersAsync(stoppingToken);
        }

        await Task.WhenAll(diffPoller.RunAsync(stoppingToken), blockPoller.RunAsync(stoppingToken));

        _logger.LogInformation(
            "Finished Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
