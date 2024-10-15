using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Nekoyume.Action.Loader;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class TxPoller : IBlockPoller
{
    private readonly MongoDbService _dbService;

    private readonly IStateService _stateService;

    private readonly ILogger _logger;

    private static readonly NCActionLoader ActionLoader = new();

    private readonly BaseActionHandler[] _handlers;
    private readonly string[] _collectionNames;

    private readonly IHeadlessGQLClient _headlessGqlClient;

    private readonly Codec _codec = new();

    public TxPoller(
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService dbService)
    {
        _headlessGqlClient = headlessGqlClient;

        _handlers =
        [
            new TableSheetStateHandler(stateService, dbService),
            // Pledge
            new PledgeStateHandler(stateService, dbService),
            // ItemSlotState
            new ItemSlotStateHandler(stateService, dbService),

            // Arena
            new ArenaStateHandler(stateService, dbService),
            // Raid
            new RaidHandler(stateService, dbService),

            // Market
            new ProductsHandler(stateService, dbService),
            // Avatar Related
            new PetStateHandler(stateService, dbService),
            new RuneSlotStateHandler(stateService, dbService),
            new StakeHandler(stateService, dbService),
        ];
        _collectionNames =
        [
            CollectionNames.GetCollectionName<StakeDocument>(),
            CollectionNames.GetCollectionName<PetStateDocument>(),
            CollectionNames.GetCollectionName<AllCombinationSlotStateDocument>(),
            CollectionNames.GetCollectionName<SheetDocument>(),
            CollectionNames.GetCollectionName<ItemSlotDocument>(),
            CollectionNames.GetCollectionName<RuneSlotDocument>(),
            CollectionNames.GetCollectionName<ArenaDocument>(),
            CollectionNames.GetCollectionName<ProductsStateDocument>(),
            CollectionNames.GetCollectionName<ProductDocument>(),
            CollectionNames.GetCollectionName<RaiderStateDocument>(),
            CollectionNames.GetCollectionName<WorldBossStateDocument>(),
            CollectionNames.GetCollectionName<WorldBossKillRewardRecordDocument>(),
        ];

        _stateService = stateService;
        _dbService = dbService;
        _logger = Log.ForContext<TxPoller>();
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Start {PollerType} background service", GetType().Name);
        var arenaCollectionName = CollectionNames.GetCollectionName<ArenaDocument>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(stoppingToken);
            // Retrieve ArenaScore Block Index. Ensure BlockPoller saves the same block index for all collections
            var syncedBlockIndex = await GetSyncedBlockIndex(arenaCollectionName, stoppingToken);

            _logger.Information(
                "Check BlockIndex synced: {SyncedBlockIndex}, current: {CurrentBlockIndex}",
                syncedBlockIndex,
                currentBlockIndex);

            if (syncedBlockIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                continue;
            }

            await ProcessBlocksAsync(syncedBlockIndex, currentBlockIndex, stoppingToken);
        }

        _logger.Information(
            "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
            GetType().Name,
            DateTime.UtcNow.Subtract(started).Minutes);
    }

    private async Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long targetBlockIndex,
        CancellationToken stoppingToken
    )
    {
        var indexDifference = Math.Abs(targetBlockIndex - syncedBlockIndex);
        const int limit = 1;

        _logger.Information(
            "Process block, target&sync: {TargetBlockIndex}&{SyncedBlockIndex}, index-diff: {IndexDiff}, limit: {Limit}",
            targetBlockIndex,
            syncedBlockIndex,
            indexDifference,
            limit);

        await ProcessTransactions(syncedBlockIndex, limit, stoppingToken);
    }

    private async Task ProcessTransactions(
        long syncedBlockIndex,
        int limit,
        CancellationToken cancellationToken)
    {
        var txsResponse = await FetchTransactionsAsync(syncedBlockIndex, limit, cancellationToken);
        if (txsResponse.NCTransactions.Count == 0)
        {
            _logger.Information("Transactions is empty");

            foreach (var collectionName in _collectionNames)
            {
                await _dbService.UpdateLatestBlockIndexAsync(
                    new MetadataDocument
                    {
                        PollerType = nameof(TxPoller),
                        CollectionName = collectionName,
                        LatestBlockIndex = syncedBlockIndex + limit
                    },
                    null,
                    cancellationToken);
            }

            return;
        }

        var blockIndex = syncedBlockIndex + limit;
        var tuples = txsResponse.NCTransactions
            .Where(tx => tx is not null)
            .Select(tx =>
                (
                    TxId: tx!.Id,
                    Signer: new Address(tx.Signer),
                    actions: tx.Actions
                        .Where(action => action is not null)
                        .Select(action => ActionLoader.LoadAction(
                            blockIndex,
                            _codec.Decode(Convert.FromHexString(action!.Raw))))
                        .ToList()
                )
            )
            .ToList();
        _logger.Information("GetTransaction Success, tx-count: {TxCount}", txsResponse.NCTransactions.Count);
        var tasks = new List<Task>();
        foreach (var (txId, signer, actions) in tuples)
        {
            foreach (var action in actions)
            {
                var (actionType, actionPlainValueInternal) = DeconstructActionPlainValue(action.PlainValue);
                foreach (var handler in _handlers)
                {
                    var task = handler.TryHandleAction(
                        blockIndex,
                        txId,
                        signer,
                        action.PlainValue,
                        actionType,
                        actionPlainValueInternal,
                        stoppingToken: cancellationToken);
                    tasks.Add(task);
                }
            }
        }

        await Task.WhenAll(tasks);

        foreach (var collectionName in _collectionNames)
        {
            await _dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = nameof(TxPoller),
                    CollectionName = collectionName,
                    LatestBlockIndex = syncedBlockIndex + limit
                },
                cancellationToken: cancellationToken);
        }
    }

    private async Task<TransactionResponse> FetchTransactionsAsync(
        long syncedBlockIndex,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = await _headlessGqlClient.GetTransactionsAsync(
            syncedBlockIndex,
            limit,
            cancellationToken);
        return result.Transaction;
    }

    /// <summary>
    /// Deconstructs the given action plain value.
    /// </summary>
    /// <param name="actionPlainValue"><see cref="Libplanet.Action.IAction.PlainValue"/></param>
    /// <returns>
    /// A tuple of two values: the first is the value of the "type_id" key, and the second is the value of the
    /// "values" key.
    /// If the given action plain value is not a dictionary, both values are null.
    /// And if the given action plain value is a dictionary but does not contain the "type_id" or "values" key,
    /// the value of the key is null.
    /// "type_id": Bencodex.Types.Text or Bencodex.Types.Integer.
    ///            (check <see cref="Nekoyume.Action.GameAction.PlainValue"/> with
    ///            <see cref="Libplanet.Action.ActionTypeAttribute"/>)
    /// "values": It can be any type of Bencodex.Types.
    /// </returns>
    private static (IValue? typeId, IValue? values) DeconstructActionPlainValue(IValue actionPlainValue)
    {
        if (actionPlainValue is not Dictionary actionPlainValueDict)
        {
            return (null, null);
        }

        var actionType = actionPlainValueDict.ContainsKey("type_id")
            ? actionPlainValueDict["type_id"]
            : null;
        var actionPlainValueInternal = actionPlainValueDict.ContainsKey("values")
            ? actionPlainValueDict["values"]
            : null;
        return (actionType, actionPlainValueInternal);
    }

    private async Task<long> GetSyncedBlockIndex(string collectionName, CancellationToken stoppingToken)
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndexAsync(
                nameof(TxPoller),
                collectionName,
                stoppingToken);
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(stoppingToken);
            _logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex);
            await _dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = nameof(TxPoller),
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                },
                null,
                stoppingToken);
            return currentBlockIndex - 1;
        }
    }
}
