using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Action;
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
        IAction action,
        IValue? actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (await TryHandleAction(blockIndex, signer, action, session, stoppingToken))
        {
            return true;
        }

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

        return await TryHandleAction(blockIndex, actionTypeStr, actionPlainValueInternal, session, stoppingToken);
    }

    [Obsolete("Use the overload with actionType instead.")]
    protected virtual Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        return Task.FromResult(false);
    }

    protected virtual Task<bool> TryHandleAction(
        long blockIndex,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        return Task.FromResult(false);
    }
}
