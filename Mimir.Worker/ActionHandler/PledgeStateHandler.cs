using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class PledgeStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^approve_pledge[0-9]*$|^end_pledge[0-9]*$|^request_pledge[0-9]*$",
        Log.ForContext<PledgeStateHandler>())
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
        if (Regex.IsMatch(actionType, "^approve_pledge[0-9]*$"))
        {
            var action = new ApprovePledge();
            action.LoadPlainValue(actionPlainValue);
            await PledgeCollectionUpdater.ApproveAsync(Store, action.PatronAddress, session, stoppingToken);   
        }

        if (Regex.IsMatch(actionType, "^end_pledge[0-9]*$"))
        {
            var action = new EndPledge();
            action.LoadPlainValue(actionPlainValue);
            await PledgeCollectionUpdater.DeleteAsync(
                Store,
                action.AgentAddress.GetPledgeAddress(),
                session,
                stoppingToken);   
        }

        if (Regex.IsMatch(actionType, "^request_pledge[0-9]*$"))
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
}
