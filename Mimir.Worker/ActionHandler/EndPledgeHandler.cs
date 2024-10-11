using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class EndPledgeHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^end_pledge[0-9]*$",
        Log.ForContext<EndPledgeHandler>())
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
        var action = new EndPledge();
        action.LoadPlainValue(actionPlainValue);
        await PledgeCollectionUpdater.DeleteAsync(
            Store,
            action.AgentAddress.GetPledgeAddress(),
            session,
            stoppingToken);
    }
}
