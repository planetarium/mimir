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
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var action = new RequestPledge();
        action.LoadPlainValue(actionPlainValue);
        await PledgeCollectionUpdater.UpsertAsync(
            Store,
            action.AgentAddress.GetPledgeAddress(),
            signer,
            false,
            action.RefillMead,
            session,
            stoppingToken);
    }
}
