using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Mimir.Worker.Util;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public abstract class BaseDiffHandler(
    string collectionName,
    Address accountAddress,
    IStateDocumentConverter stateDocumentConverter,
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    ILogger logger
) : BackgroundService
{
    protected const string PollerType = "DiffPoller";
    protected static readonly Codec Codec = new();

    protected readonly StateGetter StateGetter = stateService.At();
    protected readonly ILogger Logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await initializerManager.WaitInitializers(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var (currentBaseIndex, currentTargetIndex, currentIndexOnChain, indexDifference) =
                    await CalculateCurrentAndTargetIndexes(stoppingToken);

                if (currentBaseIndex >= currentTargetIndex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);
                    continue;
                }

                Logger.Information(
                    "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
                    collectionName,
                    currentIndexOnChain,
                    indexDifference,
                    currentBaseIndex,
                    currentTargetIndex
                );

                var diffContext = await ProduceByAccount(
                    stoppingToken,
                    currentBaseIndex,
                    currentTargetIndex
                );
                await ConsumeAsync(diffContext, stoppingToken);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error occurred.");
            }
        }
    }

    protected virtual async Task<(
        long CurrentBaseIndex,
        long CurrentTargetIndex,
        long CurrentIndexOnChain,
        long IndexDifference
    )> CalculateCurrentAndTargetIndexes(CancellationToken stoppingToken)
    {
        var syncedIndex = await GetSyncedBlockIndex(stoppingToken);
        var currentBaseIndex = syncedIndex;
        Logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedIndex
        );

        var currentIndexOnChain = await stateService.GetLatestIndex(stoppingToken, accountAddress);
        var indexDifference = currentIndexOnChain - currentBaseIndex;
        var limit =
            collectionName == CollectionNames.GetCollectionName<InventoryDocument>()
            || collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
                ? 1
                : 15;
        var currentTargetIndex =
            currentBaseIndex + (indexDifference > limit ? limit : indexDifference);

        return (currentBaseIndex, currentTargetIndex, currentIndexOnChain, indexDifference);
    }

    protected virtual async Task<DiffContext> ProduceByAccount(
        CancellationToken stoppingToken,
        long currentBaseIndex,
        long currentTargetIndex
    )
    {
        var (result, _) = await headlessGqlClient.GetAccountDiffsAsync(
            currentBaseIndex,
            currentTargetIndex,
            accountAddress,
            stoppingToken
        );

        return new DiffContext
        {
            AccountAddress = accountAddress,
            CollectionName = collectionName,
            DiffResponse = result,
            TargetBlockIndex = currentTargetIndex
        };
    }

    private async Task ConsumeAsync(DiffContext diffContext, CancellationToken stoppingToken)
    {
        if (!diffContext.DiffResponse.AccountDiffs.Any())
        {
            Logger.Information("{CollectionName}: No diffs", diffContext.CollectionName);
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = PollerType,
                    CollectionName = diffContext.CollectionName,
                    LatestBlockIndex = diffContext.TargetBlockIndex
                }
            );
            return;
        }

        await ProcessStateDiff(
            stateDocumentConverter,
            diffContext.DiffResponse,
            diffContext.TargetBlockIndex,
            stoppingToken
        );

        await dbService.UpdateLatestBlockIndexAsync(
            new MetadataDocument
            {
                PollerType = PollerType,
                CollectionName = diffContext.CollectionName,
                LatestBlockIndex = diffContext.TargetBlockIndex
            },
            null,
            stoppingToken
        );
    }

    protected virtual async Task ProcessStateDiff(
        IStateDocumentConverter converter,
        GetAccountDiffsResponse diffResponse,
        long blockIndex,
        CancellationToken stoppingToken
    )
    {
        var documents = new List<MimirBsonDocument>();
        foreach (var diff in diffResponse.AccountDiffs)
        {
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);
                var document = await CreateDocumentAsync(
                    converter,
                    blockIndex,
                    address,
                    Codec.Decode(Convert.FromHexString(diff.ChangedState)));
                documents.Add(document);
            }
        }

        Logger.Information(
            "{DiffCount} Handle in {Handler} Converted {Count} States",
            diffResponse.AccountDiffs.Count(),
            converter.GetType().Name,
            documents.Count
        );

        if (documents.Count > 0)
            await dbService.UpsertStateDataManyAsync(
                collectionName,
                documents,
                cancellationToken: stoppingToken
            );
    }

    protected virtual async Task<long> GetSyncedBlockIndex(CancellationToken stoppingToken)
    {
        try
        {
            var syncedBlockIndex = await dbService.GetLatestBlockIndexAsync(
                PollerType,
                collectionName,
                stoppingToken
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await stateService.GetLatestIndex(
                stoppingToken,
                accountAddress
            );
            Logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = PollerType,
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                },
                cancellationToken: stoppingToken
            );
            return currentBlockIndex - 1;
        }
    }

    protected virtual async Task<MimirBsonDocument> CreateDocumentAsync(
        IStateDocumentConverter converter,
        long blockIndex,
        Address address,
        IValue rawState)
    {
        var pair = new AddressStatePair
        {
            BlockIndex = blockIndex,
            Address = address,
            RawState = rawState,
        };
        return converter.ConvertToDocument(pair);
    }
}
