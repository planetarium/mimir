using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class EndPledgeHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^end_pledge[0-9]*$",
        Log.ForContext<EndPledgeHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not EndPledge endPledge)
        {
            return false;
        }

        Logger.Information("Handle end_pledge, ends pledge with agent: {AgentAddress}", endPledge.AgentAddress);

        await PledgeCollectionUpdater.DeleteAsync(
            Store, endPledge.AgentAddress.GetPledgeAddress(), session, stoppingToken);

        return true;
    }
}
