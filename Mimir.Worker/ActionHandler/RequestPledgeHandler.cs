using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RequestPledgeHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^request_pledge[0-9]*$",
        Log.ForContext<RequestPledgeHandler>())
{
    private static readonly RequestPledge Action = new();

    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        Action.LoadPlainValue(actionPlainValue);
        await PledgeCollectionUpdater.UpsertAsync(
            Store,
            Action.AgentAddress.GetPledgeAddress(),
            signer,
            false,
            Action.RefillMead,
            session,
            stoppingToken);
        return true;
    }
}
