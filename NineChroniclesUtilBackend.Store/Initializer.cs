using NineChroniclesUtilBackend.Store.Events;
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
        _scrapper = new ArenaScrapper(scrapperLogger, _stateService, client);
        _scrapper.OnDataCollected += HandleDataCollected;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _scrapper.ExecuteAsync();
        await _store.FlushAsync();
        await _store.LinkAvatarsToArenaAsync();

        _store.Result.TotalElapsedMinutes = DateTime.UtcNow.Subtract(_store.Result.StartTime).Minutes;

        _logger.LogInformation($"Scrapper Result: {_scrapper.Result}");
        _logger.LogInformation($"Store Result: {_store.Result}");
    }

    private async void HandleDataCollected(object sender, ArenaDataCollectedEventArgs e)
    {
        _store.AddArenaData(e.ArenaData);
        _store.AddAvatarData(e.AvatarData);

        _logger.LogInformation("{avatarAddress} Data Collected", e.AvatarData.Avatar.address);
    }
}