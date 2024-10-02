using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ApprovePledgeHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^approve_pledge[0-9]*$",
        Log.ForContext<ApprovePledgeHandler>()
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
        if (action is not ApprovePledge approvePledge)
        {
            return false;
        }

        Logger.Information(
            "Handle approve_pledge, approve contracting with patron: {PatronAddress}",
            approvePledge.PatronAddress);

        await PledgeCollectionUpdater.ApproveAsync(Store, signer, session, stoppingToken);

        return true;
    }
}
