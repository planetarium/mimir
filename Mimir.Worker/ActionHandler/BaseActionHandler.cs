using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.ActionHandler;

public abstract class BaseActionHandler<TMimirBsonDocument>(
    IStateService stateService,
    MongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    [StringSyntax(StringSyntaxAttribute.Regex)]
    string actionTypeRegex,
    ILogger logger) : BackgroundService
    where TMimirBsonDocument : MimirBsonDocument
{
    private readonly Codec Codec = new();

    protected readonly IStateService StateService = stateService;

    protected readonly StateGetter StateGetter = stateService.At();

    protected readonly MongoDbService Store = store;

    protected readonly ILogger Logger = logger;
    
    
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        Logger.Information("Start {PollerType} background service", GetType().Name);
        
        await initializerManager.WaitInitializers(stoppingToken);

        var collectionName = CollectionNames.GetCollectionName<TMimirBsonDocument>();

        while (!stoppingToken.IsCancellationRequested)
        {
            // 'currentBlockIndex' means the tip block index of the network.
            long? currentBlockIndex = null;
            // Retrieve ArenaScore Block Index. Ensure BlockPoller saves the same block index for all collections.
            long? syncedBlockIndex = null;
            long? indexDifference = null;
            try
            {
                currentBlockIndex = await StateService.GetLatestIndex(stoppingToken);
                syncedBlockIndex = await GetSyncedBlockIndex(collectionName, stoppingToken);

                Logger.Information(
                    "Check BlockIndex synced: {SyncedBlockIndex}, current: {CurrentBlockIndex}",
                    syncedBlockIndex,
                    currentBlockIndex);

                // Because of Libplanet's sloth feature, we can fetch blocks up to tip - 1.
                // So 'maxFetchableBlockIndex' means the maximum block index that can be fetched.
                var maxFetchableBlockIndex = currentBlockIndex - 1;

                // 'targetBlockIndex' means the block index to fetch.
                // In other words, the next block index of the 'syncedBlockIndex'.
                var targetBlockIndex = syncedBlockIndex + 1;

                if (targetBlockIndex > maxFetchableBlockIndex)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                    continue;
                }

                indexDifference = Math.Abs(currentBlockIndex.Value - syncedBlockIndex.Value);

                Logger.Information(
                    "Process block, synced&tip: {SyncedBlockIndex}&{TipBlockIndex}, index-diff: {IndexDiff}",
                    syncedBlockIndex,
                    currentBlockIndex,
                    indexDifference);

                await ProcessBlocksAsync(targetBlockIndex.Value, stoppingToken);
            }
            catch (Exception e)
            {
                Logger.Error(
                    e,
                    "Unexpected error, synced&tip: {SyncedBlockIndex}&{TipBlockIndex}, index-diff: {IndexDiff}",
                    syncedBlockIndex ?? -1,
                    currentBlockIndex ?? -1,
                    indexDifference ?? -1);
                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
            }
        }

        Logger.Information(
            "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
            GetType().Name,
            DateTime.UtcNow.Subtract(started).Minutes);
    }
    
    /// <summary>
    /// Process block at the given <see cref="blockIndex"/>.
    /// </summary>
    /// <param name="blockIndex">The block index to process.</param>
    /// <param name="stoppingToken"></param>
    private async Task ProcessBlocksAsync(
        long blockIndex,
        CancellationToken stoppingToken
    )
    {
        await ProcessTransactions(blockIndex, stoppingToken);
    }

    private async Task ProcessTransactions(
        long blockIndex,
        CancellationToken cancellationToken)
    {
        var txsResponse = await FetchTransactionsAsync(blockIndex, cancellationToken);

        Logger.Information("GetTransaction Success, tx-count: {TxCount}", txsResponse.NCTransactions.Count);
        await HandleTransactionsAsync(
            blockIndex,
            txsResponse,
            cancellationToken);
    }

    private async Task<TransactionResponse> FetchTransactionsAsync(
        long syncedBlockIndex,
        CancellationToken cancellationToken)
    {
        var result = await headlessGqlClient.GetTransactionsAsync(
            syncedBlockIndex,
            cancellationToken);
        return result.Transaction;
    }

    private async Task<long> GetSyncedBlockIndex(string collectionName, CancellationToken stoppingToken)
    {
        try
        {
            var syncedBlockIndex = await Store.GetLatestBlockIndexAsync(
                "TxPoller",
                collectionName,
                stoppingToken);
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await StateService.GetLatestIndex(stoppingToken);
            Logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex);
            await Store.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = "TxPoller",
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                },
                null,
                stoppingToken);
            return currentBlockIndex - 1;
        }
    }

    private async Task HandleTransactionsAsync(
        long blockIndex,
        TransactionResponse transactionResponse,
        CancellationToken cancellationToken)
    {
        var tuples = transactionResponse.NCTransactions
            .Where(tx => tx is not null)
            .Select(tx =>
                (
                    TxId: tx!.Id,
                    Signer: new Address(tx.Signer),
                    actions: tx.Actions
                        .Where(action => action is not null)
                        .Select(action => Codec.Decode(Convert.FromHexString(action!.Raw)))
                        .ToList()
                )
            )
            .ToList();
        
        var documents = new List<WriteModel<BsonDocument>>();
        foreach (var (txId, signer, actions) in tuples)
        {
            foreach (var action in actions)
            {
                var (actionType, actionValues) = DeconstructActionPlainValue(action);
                documents.AddRange(await HandleActionAsync(
                    blockIndex,
                    txId,
                    signer,
                    action,
                    actionType,
                    actionValues,
                    stoppingToken: cancellationToken));
            }
        }

        if (documents.Count > 0)
        {
            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<TMimirBsonDocument>(),
                documents,
                null,
                cancellationToken);   
        }
        
        await Store.UpdateLatestBlockIndexAsync(
            new MetadataDocument
            {
                PollerType = "TxPoller",
                CollectionName = CollectionNames.GetCollectionName<TMimirBsonDocument>(),
                LatestBlockIndex = blockIndex,
            },
            null,
            cancellationToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        string txId,
        Address signer,
        IValue actionPlainValue,
        IValue? actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var actionTypeStr = actionType switch
        {
            Integer integer => integer.ToString(),
            Text text => (string)text,
            _ => null
        };
        if (actionTypeStr is null || !Regex.IsMatch(actionTypeStr, actionTypeRegex))
        {
            return [];
        }

        Logger.Information(
            "Handling action. {BlockIndex}, {TxId}, {ActionType}",
            blockIndex,
            txId,
            actionTypeStr);

        var documents = await HandleActionAsync(
            blockIndex,
            signer,
            actionPlainValue,
            actionTypeStr,
            actionPlainValueInternal,
            session,
            stoppingToken);

        Logger.Information(
            "Finished handling action. {BlockIndex}, {TxId}, {ActionType}",
            blockIndex,
            txId,
            actionTypeStr);

        return documents;
    }

    // FIXME: `string actionType` argument may can be removed.
    protected abstract Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default);
}
