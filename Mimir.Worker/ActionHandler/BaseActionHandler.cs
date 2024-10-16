using System.Diagnostics.CodeAnalysis;
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
    [StringSyntax(StringSyntaxAttribute.Regex)]
    string actionTypeRegex,
    ILogger logger)
{
    protected readonly IStateService StateService = stateService;

    protected readonly StateGetter StateGetter = stateService.At();

    protected readonly MongoDbService Store = store;

    protected readonly ILogger Logger = logger;

    public async Task HandleActionAsync(
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
            return;
        }

        Logger.Information(
            "Handling action. {BlockIndex}, {TxId}, {ActionType}",
            blockIndex,
            txId,
            actionTypeStr);

        await HandleActionAsync(
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
    }

    // FIXME: `string actionType` argument may can be removed.
    protected abstract Task HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default);
}
