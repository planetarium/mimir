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
            "Attempting to handle action of type {ActionType} at block index {BlockIndex} for signer {Signer}",
            actionTypeStr,
            blockIndex,
            signer);
        bool result;
        try
        {
            result = await TryHandleAction(
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
                "Failed to load plain value for action: {ActionType} at block index {BlockIndex} for signer {Signer}",
                actionType,
                blockIndex,
                signer);
            return false;
        }

        Logger.Information(
            "Successfully handled action of type {ActionType} at block index {BlockIndex} for signer {Signer}",
            actionTypeStr,
            blockIndex,
            signer);
        return result;
    }

    // FIXME: `string actionType` argument may can be removed.
    protected virtual Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        return Task.FromResult(false);
    }
}
