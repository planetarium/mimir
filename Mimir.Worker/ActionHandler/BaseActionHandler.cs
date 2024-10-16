using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.ActionHandler;

public abstract class BaseActionHandler<TMimirBsonDocument>(
    IStateService stateService,
    MongoDbService store,
    [StringSyntax(StringSyntaxAttribute.Regex)]
    string actionTypeRegex,
    ILogger logger) : IActionHandler
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

        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<TMimirBsonDocument>(),
            documents,
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
