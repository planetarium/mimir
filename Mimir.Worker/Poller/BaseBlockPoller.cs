using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using StrawberryShake;

namespace Mimir.Worker.Poller;

public abstract class BaseBlockPoller
{
    protected readonly MongoDbService _store;
    protected readonly ILogger _logger;
    protected readonly IStateService _stateService;
    protected readonly string _pollerType;

    protected BaseBlockPoller(
        ILogger logger,
        IStateService stateService,
        MongoDbService store,
        string pollerType
    )
    {
        _logger = logger;
        _stateService = stateService;
        _store = store;
        _pollerType = pollerType;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.LogInformation("Start {PollerType} background service", GetType().Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            var syncedBlockIndex = await GetSyncedBlockIndex(currentBlockIndex);

            _logger.LogInformation(
                "Check BlockIndex synced: {SyncedBlockIndex}, current: {CurrentBlockIndex}",
                syncedBlockIndex,
                currentBlockIndex
            );

            if (syncedBlockIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(7000), stoppingToken);
                continue;
            }

            await ProcessBlocksAsync(syncedBlockIndex, currentBlockIndex, stoppingToken);
        }

        _logger.LogInformation(
            "Finished {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
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
            _logger.LogInformation(
                "Metadata collection not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await _store.UpdateLatestBlockIndex(currentBlockIndex - 1, _pollerType);
            return currentBlockIndex - 1;
        }
    }
}
