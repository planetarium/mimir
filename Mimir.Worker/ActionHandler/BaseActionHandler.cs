using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Poller;
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
    [StringSyntax(StringSyntaxAttribute.Regex)]
    string actionTypeRegex,
    ILogger logger) : BackgroundService, IActionHandler
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
        while (!stoppingToken.IsCancellationRequested)
        {
            var collectionName = CollectionNames.GetCollectionName<TMimirBsonDocument>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var currentBlockIndex = await StateService.GetLatestIndex(stoppingToken);
                // Retrieve ArenaScore Block Index. Ensure BlockPoller saves the same block index for all collections
                var syncedBlockIndex = await GetSyncedBlockIndex(collectionName, stoppingToken);

                Logger.Information(
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

            Logger.Information(
                "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
                GetType().Name,
                DateTime.UtcNow.Subtract(started).Minutes);
        }
    }
    
    private async Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long targetBlockIndex,
        CancellationToken stoppingToken
    )
    {
        var indexDifference = Math.Abs(targetBlockIndex - syncedBlockIndex);
        const int limit = 1;

        Logger.Information(
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

        var blockIndex = syncedBlockIndex + limit;
        Logger.Information("GetTransaction Success, tx-count: {TxCount}", txsResponse.NCTransactions.Count);
        await HandleTransactionsAsync(
            blockIndex,
            txsResponse,
            cancellationToken);
    }

    private async Task<TransactionResponse> FetchTransactionsAsync(
        long syncedBlockIndex,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = await headlessGqlClient.GetTransactionsAsync(
            syncedBlockIndex,
            limit,
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

    public async Task HandleTransactionsAsync(
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
