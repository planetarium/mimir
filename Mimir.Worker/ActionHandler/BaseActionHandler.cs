using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.Util;
using Mimir.Worker.Services;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.ActionHandler;

public abstract class BaseActionHandler(
    IStateService stateService,
    MongoDbService store,
    string actionTypeRegex,
    ILogger logger)
{
    protected readonly IStateService StateService = stateService;

    protected readonly StateGetter StateGetter = new(stateService);

    protected readonly MongoDbService Store = store;

    protected readonly ILogger Logger = logger;

    public async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IValue? actionType,
        IValue? actionPlainValueInternal)
    {
        try
        {
            await HandleAction(blockIndex, signer, action);
            return true;
        }
        catch (NotImplementedException)
        {
            // ignored
        }

        var actionTypeStr = actionType switch
        {
            Integer integer => integer.ToString(),
            Text text => (string)text,
            _ => null
        };
        if (actionTypeStr is null ||
            !Regex.IsMatch(actionTypeStr, actionTypeRegex))
        {
            return false;
        }

        try
        {
            await HandleAction(
                actionTypeStr,
                blockIndex,
                actionPlainValueInternal);
        }
        catch (NotImplementedException)
        {
            // ignored
            return false;
        }

        return false;
    }

    protected virtual Task HandleAction(
        long blockIndex,
        Address signer,
        IAction action)
    {
        throw new NotImplementedException();
    }

    protected virtual Task HandleAction(
        string actionType,
        long processBlockIndex,
        IValue? actionPlainValueInternal)
    {
        throw new NotImplementedException();
    }
}
