using System.Collections.Concurrent;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffProducer
{
    private readonly ConcurrentQueue<DiffContext> _queue;
    protected readonly MongoDbService _dbService;
    protected readonly IStateService _stateService;
    protected readonly string _pollerType;
    protected readonly ILogger _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;

    public DiffProducer(
        ConcurrentQueue<DiffContext> queue,
        IStateService stateService,
        HeadlessGQLClient headlessGqlClient,
        MongoDbService dbService
    )
    {
        _headlessGqlClient = headlessGqlClient;

        _stateService = stateService;
        _pollerType = "DiffBlockPoller";
        _logger = Log.ForContext<DiffProducer>();
        _dbService = dbService;
        _queue = queue;
    }

    public async Task ProduceAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        foreach (var accountAddress in AddressHandlerMappings.HandlerMappings.Keys)
        {
            tasks.Add(ProduceByAccount(stoppingToken, accountAddress));
        }
        await Task.WhenAll(tasks);
    }

    private async Task ProduceByAccount(CancellationToken stoppingToken, Address accountAddress)
    {
        var collectionName = CollectionNames.GetCollectionName(accountAddress);

        var syncedBlockIndex = await GetSyncedBlockIndex(collectionName);
        var currentTargetIndex = syncedBlockIndex;
        _logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedBlockIndex
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            var currentBaseIndex = currentTargetIndex;
            long indexDifference = Math.Abs(currentBaseIndex - currentBlockIndex);
            int limit =
                collectionName == CollectionNames.GetCollectionName<InventoryDocument>()
                || collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
                    ? 1
                    : 15;
            currentTargetIndex =
                currentTargetIndex + (indexDifference > limit ? limit : indexDifference);

            if (currentBaseIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                continue;
            }

            _logger.Information(
                "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
                collectionName,
                currentBlockIndex,
                indexDifference,
                currentBaseIndex,
                currentTargetIndex
            );

            for (int i = 0; i < 3; i++)
            {
                var diffResult = await FetchDiffAsync(
                    currentBaseIndex,
                    currentTargetIndex,
                    accountAddress,
                    stoppingToken
                );
                if (diffResult is null)
                {
                    _logger.Error("Failed to get diffs. retry {Count}", i);
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                }
                else
                {
                    while (_queue.Count() < 100)
                    {
                        _logger.Error("Queue is full, wait 1000");
                        await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                    }
                    _queue.Enqueue(
                        new DiffContext()
                        {
                            Diffs = diffResult,
                            AccountAddress = accountAddress,
                            TargetBlockIndex = currentTargetIndex,
                            CollectionName = collectionName
                        }
                    );
                    break;
                }
            }
        }
    }

    private async Task<IEnumerable<IGetAccountDiffs_AccountDiffs>?> FetchDiffAsync(
        long syncedBlockIndex,
        long targetIndex,
        Address accountAddress,
        CancellationToken stoppingToken
    )
    {
        var diffResult = await _headlessGqlClient.GetAccountDiffs.ExecuteAsync(
            syncedBlockIndex,
            targetIndex,
            accountAddress.ToString(),
            stoppingToken
        );

        if (diffResult.Data is null)
        {
            var errors = diffResult.Errors.Select(e => e.Message);
            _logger.Error("Failed to get diffs. response data is null. errors:\n{Errors}", errors);
            return null;
        }

        return diffResult.Data.AccountDiffs;
    }

    public async Task<long> GetSyncedBlockIndex(string collectionName)
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndex(
                _pollerType,
                collectionName
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex();
            _logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await _dbService.UpdateLatestBlockIndex(
                new MetadataDocument
                {
                    PollerType = _pollerType,
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                }
            );
            return currentBlockIndex - 1;
        }
    }
}
