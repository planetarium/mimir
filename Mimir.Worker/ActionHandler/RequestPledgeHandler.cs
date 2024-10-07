using Libplanet.Action;
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
        Log.ForContext<RequestPledgeHandler>()
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
        if (action is not RequestPledge requestPledge)
        {
            return false;
        }

        Logger.Information(
            "Handle request_pledge, request contracting with agent: {AgentAddress}",
            requestPledge.AgentAddress);

        await PledgeCollectionUpdater.UpsertAsync(
            Store,
            requestPledge.AgentAddress.GetPledgeAddress(),
            signer,
            false,
            requestPledge.RefillMead,
            session,
            stoppingToken);

        return true;
    }
}
