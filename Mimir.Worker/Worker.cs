using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly DiffMongoDbService _store;
    private readonly ILogger<Worker> _logger;
    private readonly ILogger<SnapshotInitializer> _initializerLogger;
    private readonly ILogger<DiffBlockPoller> _blockPollerLogger;
    private readonly IStateService _stateService;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly string _snapshotPath;

    public Worker(
        ILogger<Worker> logger,
        ILogger<DiffBlockPoller> blockPollerLogger,
        ILogger<SnapshotInitializer> initializerLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        DiffMongoDbService store,
        string snapshotPath
    )
    {
        _logger = logger;
        _initializerLogger = initializerLogger;
        _blockPollerLogger = blockPollerLogger;
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
        _snapshotPath = snapshotPath;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        var initializer = new SnapshotInitializer(_initializerLogger, _store, _snapshotPath);
        var poller = new DiffBlockPoller(
            _blockPollerLogger,
            _headlessGqlClient,
            _stateService,
            _store
        );

        await initializer.RunAsync(stoppingToken);
        // await poller.RunAsync(stoppingToken);

        _logger.LogInformation(
            "Finished Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
