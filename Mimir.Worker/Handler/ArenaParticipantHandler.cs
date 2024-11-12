using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class ArenaParticipantHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler(
        "arena_participant_0_0",
        Addresses.GetArenaParticipantAccountAddress(0, 0),
        new ArenaParticipantDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<ArenaParticipantHandler>())
{
    private const string MetadataCollectionName = "arena_participant";

    private readonly MongoDbService _dbService = dbService;
    private readonly IStateService _stateService = stateService;
    private readonly IHeadlessGQLClient _headlessGqlClient = headlessGqlClient;

    private ArenaSheet? _arenaSheet;

    private ArenaSheet ArenaSheet
    {
        get
        {
            if (_arenaSheet is null)
            {
                _arenaSheet = _dbService.GetSheetAsync<ArenaSheet>().Result;
                if (_arenaSheet is null)
                {
                    throw new InvalidOperationException("ArenaSheet could not be retrieved from the database.");
                }
            }

            return _arenaSheet;
        }
    }

    protected override async Task<(long, long, long, long)> CalculateCurrentAndTargetIndexes(
        CancellationToken stoppingToken)
    {
        var syncedIndex = await GetSyncedBlockIndex(stoppingToken);
        var currentBaseIndex = syncedIndex;

        var (accountAddress, collectionName) = GetArenaParticipantInfo(syncedIndex);
        Logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedIndex);

        var currentIndexOnChain = await _stateService.GetLatestIndex(stoppingToken, accountAddress);
        var indexDifference = currentIndexOnChain - currentBaseIndex;
        var limit = collectionName == CollectionNames.GetCollectionName<InventoryDocument>() ||
                    collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
            ? 1
            : 15;
        var currentTargetIndex = currentBaseIndex + (indexDifference > limit ? limit : indexDifference);
        if (currentBaseIndex >= currentTargetIndex)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }

        Logger.Information(
            "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
            collectionName,
            currentIndexOnChain,
            indexDifference,
            currentBaseIndex,
            currentTargetIndex);

        return (currentBaseIndex, currentTargetIndex, currentIndexOnChain, indexDifference);
    }

    protected override async Task ConsumeAsync(DiffContext diffContext, CancellationToken stoppingToken)
    {
        if (!diffContext.DiffResponse.AccountDiffs.Any())
        {
            Logger.Information("{CollectionName}: No diffs", diffContext.CollectionName);
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = PollerType,
                    CollectionName = MetadataCollectionName,
                    LatestBlockIndex = diffContext.TargetBlockIndex
                }
            );
            return;
        }

        await ProcessStateDiff(
            StateDocumentConverter,
            diffContext.DiffResponse,
            diffContext.TargetBlockIndex,
            stoppingToken
        );

        await dbService.UpdateLatestBlockIndexAsync(
            new MetadataDocument
            {
                PollerType = PollerType,
                CollectionName = MetadataCollectionName,
                LatestBlockIndex = diffContext.TargetBlockIndex
            },
            null,
            stoppingToken
        );
    }

    protected override async Task<long> GetSyncedBlockIndex(CancellationToken stoppingToken)
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndexAsync(
                PollerType,
                MetadataCollectionName,
                stoppingToken);
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(
                stoppingToken,
                accountAddress: null);
            Logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex);
            await _dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = PollerType,
                    CollectionName = MetadataCollectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                },
                cancellationToken: stoppingToken
            );
            return currentBlockIndex - 1;
        }
    }

    protected override async Task ProcessStateDiff(
        IStateDocumentConverter converter,
        GetAccountDiffsResponse diffResponse,
        long blockIndex,
        CancellationToken stoppingToken)
    {
        var documents = new List<MimirBsonDocument>();
        foreach (var diff in diffResponse.AccountDiffs)
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);
                var pair = new AddressStatePair
                {
                    Address = address,
                    RawState = Codec.Decode(Convert.FromHexString(diff.ChangedState))
                };
                var document = converter.ConvertToDocument(pair);
                documents.Add(document);
            }

        Logger.Information(
            "{DiffCount} Handle in {Handler} Converted {Count} States",
            diffResponse.AccountDiffs.Count,
            converter.GetType().Name,
            documents.Count);

        if (documents.Count > 0)
        {
            await _dbService.UpsertStateDataManyAsync(
                GetCollectionName(blockIndex),
                documents,
                createCollectionIfNotExists: true,
                cancellationToken: stoppingToken);
        }
    }

    protected override async Task<DiffContext> ProduceByAccount(
        CancellationToken stoppingToken,
        long currentBaseIndex,
        long currentTargetIndex)
    {
        var (accountAddress, collectionName) = GetArenaParticipantInfo(currentBaseIndex);
        var result = await _headlessGqlClient.GetAccountDiffsAsync(
            currentBaseIndex,
            currentTargetIndex,
            accountAddress,
            stoppingToken);

        return new DiffContext
        {
            AccountAddress = accountAddress,
            CollectionName = collectionName,
            DiffResponse = result,
            TargetBlockIndex = currentTargetIndex
        };
    }

    private string GetCollectionName(long blockIndex)
    {
        var roundData = ArenaSheet.GetRoundByBlockIndex(blockIndex);
        return $"arena_participant_{roundData.ChampionshipId}_{roundData.Round}";
    }

    private (Address accountAddress, string collectionName) GetArenaParticipantInfo(long blockIndex)
    {
        var roundData = ArenaSheet.GetRoundByBlockIndex(blockIndex);
        var accountAddress = Addresses.GetArenaParticipantAccountAddress(roundData.ChampionshipId, roundData.Round);
        var collectionName = GetCollectionName(blockIndex);
        return (accountAddress, collectionName);
    }
}
