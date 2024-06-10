using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Nekoyume.Model.State;
using StrawberryShake;

namespace Mimir.Worker.Poller;

public class BlockPoller : BaseBlockPoller
{
    private readonly Dictionary<string, BaseActionHandler> _handlers;

    private readonly IHeadlessGQLClient _headlessGqlClient;

    private readonly Codec Codec = new();

    public BlockPoller(
        ILogger<BlockPoller> logger,
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService store
    )
        : base(logger, stateService, store, "BlockPoller")
    {
        _headlessGqlClient = headlessGqlClient;

        var handlers = new List<BaseActionHandler>
        {
            new BattleArenaHandler(stateService, store),
            new PatchTableHandler(stateService, store)
        };
        _handlers = handlers.ToDictionary(handler => handler.ActionRegex);
    }

    protected override async Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long currentBlockIndex,
        CancellationToken stoppingToken
    )
    {
        long indexDifference = Math.Abs(currentBlockIndex - syncedBlockIndex);
        int limit = (int)(indexDifference > 100 ? 100 : indexDifference);

        await ProcessTransactions(syncedBlockIndex, limit, stoppingToken);
    }

    private async Task ProcessTransactions(
        long syncedBlockIndex,
        int limit,
        CancellationToken cancellationToken
    )
    {
        var operationResult = await _headlessGqlClient.GetTransactions.ExecuteAsync(
            syncedBlockIndex,
            limit,
            cancellationToken
        );
        if (operationResult.Data is null)
        {
            HandleErrors(operationResult);
            return;
        }

        var txs = operationResult.Data.Transaction.NcTransactions;
        if (txs is null || txs.Count == 0)
        {
            return;
        }

        List<List<string>> actionsList = txs.Select(tx =>
                tx.Actions.Select(action => action.Raw).ToList()
            )
            .ToList();

        foreach (var actions in actionsList)
        {
            foreach (var rawAction in actions)
            {
                var action = (Dictionary)Codec.Decode(Convert.FromHexString(rawAction));
                var actionType = (Text)action["type_id"];
                var actionValues = action["values"];

                foreach (var handler in _handlers.Values)
                {
                    if (Regex.IsMatch(actionType, handler.ActionRegex))
                    {
                        await handler.HandleAction(
                            syncedBlockIndex + limit,
                            (Dictionary)actionValues
                        );
                        break;
                    }
                }
            }
        }

        await _store.UpdateLatestBlockIndex(syncedBlockIndex + limit, _pollerType);
    }

    private static void HandleErrors(IOperationResult operationResult)
    {
        var errors = operationResult.Errors.Select(e => e.Message);
        Serilog.Log.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    }
}
