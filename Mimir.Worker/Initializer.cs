using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class Initializer : BackgroundService
{
    private readonly MongoDbStore _store;
    private readonly ArenaScrapper _arenaScrapper;
    private readonly TableSheetScrapper _tableSheetScrapper;
    private readonly ILogger<Initializer> _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;

    public Initializer(
        ILogger<Initializer> logger,
        ILogger<ArenaScrapper> arenaScrapperLogger,
        ILogger<TableSheetScrapper> tableSheetScrapperLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbStore store)
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
        _arenaScrapper = new ArenaScrapper(arenaScrapperLogger, _stateService, _store);
        _tableSheetScrapper = new TableSheetScrapper(tableSheetScrapperLogger, _stateService, _store);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        var latestBlockIndex = await _stateService.GetLatestIndex();

        await _tableSheetScrapper.ExecuteAsync(latestBlockIndex, stoppingToken);   
        await _arenaScrapper.ExecuteAsync(latestBlockIndex, stoppingToken);
        
        await _store.UpdateLatestBlockIndex(latestBlockIndex);

        _logger.LogInformation(
                "Finished Initializer background service. Elapsed {TotalElapsedMinutes} minutes",
                DateTime.UtcNow.Subtract(started).Minutes);

        var poller = new BlockPoller(_stateService, _headlessGqlClient, _store);
        await poller.RunAsync(stoppingToken);
    }
}
