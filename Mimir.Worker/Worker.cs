using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly DiffMongoDbService _store;
    private readonly ILogger<Worker> _logger;
    private readonly ILogger<DiffBlockPoller> _blockPollerLogger;
    private readonly IStateService _stateService;
    private readonly HeadlessGQLClient _headlessGqlClient;

    public Worker(
        ILogger<Worker> logger,
        ILogger<DiffBlockPoller> blockPollerLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        DiffMongoDbService store)
    {
        _logger = logger;
        _blockPollerLogger = blockPollerLogger;
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        var poller = new DiffBlockPoller(_blockPollerLogger, _headlessGqlClient, _stateService, _store);

        await poller.RunAsync(stoppingToken);

        _logger.LogInformation(
                "Finished Initializer background service. Elapsed {TotalElapsedMinutes} minutes",
                DateTime.UtcNow.Subtract(started).Minutes);

    }
}
