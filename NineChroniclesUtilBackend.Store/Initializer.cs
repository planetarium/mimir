using NineChroniclesUtilBackend.Store.Scrapper;
using NineChroniclesUtilBackend.Store.Client;
using NineChroniclesUtilBackend.Store.Services;

namespace NineChroniclesUtilBackend.Store;

public class Initializer : BackgroundService
{
    private readonly MongoDbStore _store;
    private readonly ArenaScrapper _scrapper;
    private readonly ILogger<Initializer> _logger;
    private readonly IStateService _stateService;

    public Initializer(
        ILogger<Initializer> logger,
        ILogger<ArenaScrapper> scrapperLogger,
        IStateService stateService,
        MongoDbStore store,
        EmptyChronicleClient client)
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _scrapper = new ArenaScrapper(scrapperLogger, _stateService, client, _store);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        if (!await _store.IsInitialized())
        {
            await _scrapper.ExecuteAsync();   
        }

        var totalElapsedMinutes = DateTime.UtcNow.Subtract(started).Minutes;
        _logger.LogInformation($"Finished Initializer background service. Elapsed {totalElapsedMinutes} minutes.");
    }
}
