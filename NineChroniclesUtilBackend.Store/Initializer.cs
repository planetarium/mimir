using HeadlessGQL;
using NineChroniclesUtilBackend.Store.Scrapper;
using NineChroniclesUtilBackend.Store.Services;

namespace NineChroniclesUtilBackend.Store;

public class Initializer : BackgroundService
{
    private readonly MongoDbStore _store;
    private readonly ArenaScrapper _scrapper;
    private readonly ILogger<Initializer> _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;

    public Initializer(
        ILogger<Initializer> logger,
        ILogger<ArenaScrapper> scrapperLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbStore store)
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _headlessGqlClient = headlessGqlClient;
        _scrapper = new ArenaScrapper(scrapperLogger, _stateService, _store);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        if (!await _store.IsInitialized())
        {
            await _scrapper.ExecuteAsync(stoppingToken);   
        }

        var totalElapsedMinutes = DateTime.UtcNow.Subtract(started).Minutes;
        _logger.LogInformation($"Finished Initializer background service. Elapsed {totalElapsedMinutes} minutes.");

        var poller = new BlockPoller(_stateService, _headlessGqlClient, _store);
        await poller.RunAsync(stoppingToken);
    }
}
