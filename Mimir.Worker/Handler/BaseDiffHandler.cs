using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public abstract class BaseDiffHandler(
    string collectionName,
    IStateDocumentConverter stateDocumentConverter,
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    ILogger logger)
    : BackgroundService
{
    private readonly Address _accountAddress = CollectionNames.GetAccountAddress(collectionName);
    private readonly Codec _codec = new();
    protected ILogger Logger { get; } = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await initializerManager.WaitInitializers(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var diffContext = await ProduceByAccount(stoppingToken);
            await ConsumeAsync(diffContext, stoppingToken);
        }
    }

    private async Task<DiffContext> ProduceByAccount(
        CancellationToken stoppingToken
    )
    {
        var syncedIndex = await GetSyncedBlockIndex(collectionName, stoppingToken);
        var currentBaseIndex = syncedIndex;
        Logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedIndex
        );

        var currentIndexOnChain = await stateService.GetLatestIndex(
            stoppingToken,
            _accountAddress
        );

        var indexDifference = currentIndexOnChain - currentBaseIndex;
        var limit =
            collectionName == CollectionNames.GetCollectionName<InventoryDocument>()
            || collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
                ? 1
                : 15;
        var currentTargetIndex =
            currentBaseIndex + (indexDifference > limit ? limit : indexDifference);

        if (currentBaseIndex >= currentTargetIndex) await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);

        Logger.Information(
            "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
            collectionName,
            currentIndexOnChain,
            indexDifference,
            currentBaseIndex,
            currentTargetIndex
        );

        var result = await headlessGqlClient.GetAccountDiffsAsync(
            currentBaseIndex,
            currentTargetIndex,
            _accountAddress,
            stoppingToken
        );

        return new DiffContext
        {
            AccountAddress = _accountAddress,
            CollectionName = collectionName,
            DiffResponse = result,
            TargetBlockIndex = currentTargetIndex
        };
    }

    private async Task ConsumeAsync(
        DiffContext diffContext,
        CancellationToken stoppingToken
    )
    {
        if (!diffContext.DiffResponse.AccountDiffs.Any())
        {
            Logger.Information("{CollectionName}: No diffs", diffContext.CollectionName);
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = "DiffPoller",
                    CollectionName = diffContext.CollectionName,
                    LatestBlockIndex = diffContext.TargetBlockIndex
                }
            );
            return;
        }

        await ProcessStateDiff(
            stateDocumentConverter,
            diffContext.AccountAddress,
            diffContext.DiffResponse,
            stoppingToken
        );

        await dbService.UpdateLatestBlockIndexAsync(
            new MetadataDocument
            {
                PollerType = "DiffPoller",
                CollectionName = diffContext.CollectionName,
                LatestBlockIndex = diffContext.TargetBlockIndex
            },
            null,
            stoppingToken
        );
    }

    private async Task ProcessStateDiff(
        IStateDocumentConverter converter,
        Address accountAddress,
        GetAccountDiffsResponse diffResponse,
        CancellationToken stoppingToken
    )
    {
        var documents = new List<MimirBsonDocument>();
        foreach (var diff in diffResponse.AccountDiffs)
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);

                var document = converter.ConvertToDocument(
                    new AddressStatePair
                    {
                        Address = address,
                        RawState = _codec.Decode(Convert.FromHexString(diff.ChangedState))
                    }
                );

                documents.Add(document);
            }

        Logger.Information(
            "{DiffCount} Handle in {Handler} Converted {Count} States",
            diffResponse.AccountDiffs.Count(),
            converter.GetType().Name,
            documents.Count
        );

        if (documents.Count > 0)
            await dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName(accountAddress),
                documents,
                null,
                stoppingToken
            );
    }

    private async Task<long> GetSyncedBlockIndex(
        string collectionName,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var syncedBlockIndex = await dbService.GetLatestBlockIndexAsync(
                "DiffPoller",
                collectionName,
                stoppingToken
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await stateService.GetLatestIndex(
                stoppingToken,
                _accountAddress
            );
            Logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = "DiffPoller",
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                },
                cancellationToken: stoppingToken
            );
            return currentBlockIndex - 1;
        }
    }
}