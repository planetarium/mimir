using System.Threading.Channels;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffProducer
{
    protected readonly MongoDbService _dbService;
    protected readonly IStateService _stateService;
    protected readonly ILogger _logger;
    private readonly TempGQLClient _headlessGqlClient;

    public DiffProducer(
        IStateService stateService,
        TempGQLClient headlessGqlClient,
        MongoDbService dbService
    )
    {
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _logger = Log.ForContext<DiffProducer>();
        _dbService = dbService;
    }

    public async Task ProduceByAccount(
        ChannelWriter<DiffContext> writer,
        CancellationToken stoppingToken,
        Address accountAddress
    )
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

            var diffResult = await FetchDiffAsync(
                currentBaseIndex,
                currentTargetIndex,
                accountAddress,
                stoppingToken
            );

            await writer.WriteAsync(
                new DiffContext()
                {
                    Diffs = diffResult,
                    AccountAddress = accountAddress,
                    TargetBlockIndex = currentTargetIndex,
                    CollectionName = collectionName
                },
                stoppingToken
            );
        }
    }

    private async Task<IEnumerable<IGetAccountDiffs_AccountDiffs>> FetchDiffAsync(
        long syncedBlockIndex,
        long targetIndex,
        Address accountAddress,
        CancellationToken stoppingToken
    )
    {
        return await RetryUtil.RequestWithRetryAsync(
            async () =>
            {
                var diffResult = await _headlessGqlClient.GetAccountDiffsAsync(
                    syncedBlockIndex,
                    targetIndex,
                    accountAddress.ToString()
                );
                var result = new List<GetAccountDiffs_AccountDiffs_StateDiff>();

                if (diffResult is null)
                {
                    throw new HttpRequestException("Response data is null.");
                }

                foreach (var diff in diffResult.accountDiffs)
                {
                    var stateDiff = new GetAccountDiffs_AccountDiffs_StateDiff(
                        diff.path,
                        diff.baseState,
                        diff.changedState
                    );
                    result.Add(stateDiff);
                }

                return result.AsReadOnly();
            },
            retryCount: 3,
            delayMilliseconds: 30_000,
            cancellationToken: stoppingToken,
            onRetry: (ex, retryAttempt) =>
                _logger.Error(ex, "Error on retry {RetryCount}.", retryAttempt)
        );
    }

    public async Task<long> GetSyncedBlockIndex(string collectionName)
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndex(
                "DiffBlockPoller",
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
                    PollerType = "DiffBlockPoller",
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                }
            );
            return currentBlockIndex - 1;
        }
    }
}
