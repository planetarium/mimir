using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class DiffBlockPoller : BackgroundService
{
    private readonly DiffMongoDbService _store;
    private readonly DiffScrapper _diffScrapper;
    private readonly ILogger<DiffBlockPoller> _logger;
    private readonly IStateService _stateService;

    public DiffBlockPoller(
        ILogger<DiffBlockPoller> logger,
        ILogger<DiffScrapper> scrapperLogger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        DiffMongoDbService store
    )
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _diffScrapper = new DiffScrapper(scrapperLogger, headlessGqlClient, _store);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.LogInformation("Start DiffBlockPoller background service");

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            long syncedBlockIndex;
            try
            {
                syncedBlockIndex = await _store.GetLatestBlockIndex();
            }
            catch (InvalidOperationException)
            {
                syncedBlockIndex = currentBlockIndex - 2;
            }
            var processBlockIndex = syncedBlockIndex + 1;

            _logger.LogInformation(
                $"Check BlockIndex process: {processBlockIndex}, current: {currentBlockIndex}"
            );

            if (processBlockIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(5000), stoppingToken);
                continue;
            }

            await _diffScrapper.ExecuteAsync(syncedBlockIndex, currentBlockIndex);
            await _store.UpdateLatestBlockIndex(currentBlockIndex);
        }
        _logger.LogInformation(
            "Finished DiffBlockPoller background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
