using NineChroniclesUtilBackend.Store.Events;
using NineChroniclesUtilBackend.Store.Scrapper;
using NineChroniclesUtilBackend.Store.Services;

namespace NineChroniclesUtilBackend.Store;

public class Worker : BackgroundService
{
    private readonly MongoDbStore _store;
    private readonly ArenaScrapper _scrapper;
    private readonly ILogger<Worker> _logger;
    private readonly IStateService _stateService;

    public Worker(ILogger<Worker> logger, IStateService stateService, MongoDbStore store)
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _scrapper = new ArenaScrapper(_stateService);
        _scrapper.OnDataCollected += HandleDataCollected;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        await _scrapper.ExecuteAsync();
    }

    private async void HandleDataCollected(object sender, ArenaDataCollectedEventArgs e)
    {
        _logger.LogInformation("{avatarAddress} Data Collected", e.AvatarData.Avatar.address);
        
        await _store.SaveArenaDataAsync(e.ArenaData);
        await _store.SaveAvatarDataAsync(e.AvatarData);
    }
}
