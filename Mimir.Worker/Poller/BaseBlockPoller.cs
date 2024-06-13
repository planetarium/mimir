using Mimir.Worker.Services;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public abstract class BaseBlockPoller
{
    protected readonly MongoDbService _store;
    protected readonly IStateService _stateService;
    protected readonly string _pollerType;
    protected readonly ILogger _logger;

    protected BaseBlockPoller(
        IStateService stateService,
        MongoDbService store,
        string pollerType,
        ILogger logger
    )
    {
        _stateService = stateService;
        _store = store;
        _pollerType = pollerType;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Start {PollerType} background service", GetType().Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            var syncedBlockIndex = await GetSyncedBlockIndex(currentBlockIndex);

            _logger.Information(
                "Check BlockIndex synced: {SyncedBlockIndex}, current: {CurrentBlockIndex}",
                syncedBlockIndex,
                currentBlockIndex
            );

            if (syncedBlockIndex >= currentBlockIndex)
            {
                _logger.Information("Already synced, sleep");
                await Task.Delay(TimeSpan.FromMilliseconds(7000), stoppingToken);
                continue;
            }

            await ProcessBlocksAsync(syncedBlockIndex, currentBlockIndex, stoppingToken);
        }

        _logger.Information(
            "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
            GetType().Name,
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }

    protected abstract Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long currentBlockIndex,
        CancellationToken stoppingToken
    );

    public async Task<long> GetSyncedBlockIndex(long currentBlockIndex)
    {
        try
        {
            var syncedBlockIndex = await _store.GetLatestBlockIndex(_pollerType);
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            _logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await _store.UpdateLatestBlockIndex(currentBlockIndex - 1, _pollerType);
            return currentBlockIndex - 1;
        }
    }
}
