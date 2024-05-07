using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class Initializer : BackgroundService
{
    private readonly MongoDbWorker _worker;
    private readonly ArenaScrapper _scrapper;
    private readonly ILogger<Initializer> _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;

    public Initializer(
        ILogger<Initializer> logger,
        ILogger<ArenaScrapper> scrapperLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbWorker worker)
    {
        _logger = logger;
        _stateService = stateService;
        _worker = worker;
        _headlessGqlClient = headlessGqlClient;
        _scrapper = new ArenaScrapper(scrapperLogger, _stateService, _worker);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        await _scrapper.ExecuteAsync(stoppingToken);   

        var totalElapsedMinutes = DateTime.UtcNow.Subtract(started).Minutes;
        _logger.LogInformation($"Finished Initializer background service. Elapsed {totalElapsedMinutes} minutes.");

        var poller = new BlockPoller(_stateService, _headlessGqlClient, _worker);
        await poller.RunAsync(stoppingToken);
    }
}
