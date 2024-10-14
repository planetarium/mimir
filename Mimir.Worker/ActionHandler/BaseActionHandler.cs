using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Driver;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.ActionHandler;

public abstract class BaseActionHandler(
    IStateService stateService,
    MongoDbService store,
    string actionTypeRegex,
    ILogger logger)
{
    protected readonly IStateService StateService = stateService;

    protected readonly StateGetter StateGetter = stateService.At();

    protected readonly MongoDbService Store = store;

    protected readonly ILogger Logger = logger;

    public async Task<bool> TryHandleAction(
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
            return false;
        }

        Logger.Information(
            "Handling action. {BlockIndex}, {TxId}, {ActionType}",
            blockIndex,
            txId,
            actionTypeStr);
        try
        {
            await HandleAction(
                blockIndex,
                signer,
                actionPlainValue,
                actionTypeStr,
                actionPlainValueInternal,
                session,
                stoppingToken);
        }
        catch (Exception e)
        {
            Logger.Fatal(
                e,
                "Failed to handling action. {BlockIndex}, {TxId}, {ActionType}",
                blockIndex,
                txId,
                actionTypeStr);
            return false;
        }

        Logger.Information(
            "Finished handling action. {BlockIndex}, {TxId}, {ActionType}",
            blockIndex,
            txId,
            actionTypeStr);
        return true;
    }

    // FIXME: `string actionType` argument may can be removed.
    protected abstract Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default);
}
