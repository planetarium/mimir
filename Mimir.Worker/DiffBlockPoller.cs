using HeadlessGQL;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class DiffBlockPoller
{
    private readonly DiffMongoDbService _store;
    private readonly DiffScrapper _diffScrapper;
    private readonly ILogger<DiffBlockPoller> _logger;
    private readonly IStateService _stateService;

    public DiffBlockPoller(
        ILogger<DiffBlockPoller> logger,
        HeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        DiffMongoDbService store
    )
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _diffScrapper = new DiffScrapper(headlessGqlClient, _store);
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.LogInformation("Start DiffBlockPoller background service");

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            long syncedBlockIndex;
            try
            {
                syncedBlockIndex = await GetSyncedBlockIndex(currentBlockIndex);
            }
            catch (System.InvalidOperationException)
            {
                _logger.LogInformation(
                    $"Metadata collection not founded, set block index to {currentBlockIndex} - 1"
                );
                syncedBlockIndex = currentBlockIndex - 1;
                await _store.UpdateLatestBlockIndex(currentBlockIndex - 1);
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
        }
        _logger.LogInformation(
            "Finished DiffBlockPoller background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }

    public async Task<long> GetSyncedBlockIndex(long currentBlockIndex)
    {
        try
        {
            var syncedBlockIndex = await _store.GetLatestBlockIndex();
            return syncedBlockIndex;
        }
        catch (System.InvalidOperationException)
        {
            _logger.LogError(
                $"Failed to get block indexes from db, Set `syncedBlockIndex` {currentBlockIndex} - 1"
            );
            return currentBlockIndex - 1;
        }
    }
}
