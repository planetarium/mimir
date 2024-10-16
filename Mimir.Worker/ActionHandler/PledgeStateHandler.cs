using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class PledgeStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler<PledgeDocument>(
        stateService,
        store,
        "^approve_pledge[0-9]*$|^end_pledge[0-9]*$|^request_pledge[0-9]*$",
        Log.ForContext<PledgeStateHandler>())
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
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
            return [PledgeCollectionUpdater.ApproveAsync(action.PatronAddress)];   
        }

        if (Regex.IsMatch(actionType, "^end_pledge[0-9]*$"))
        {
            var action = new EndPledge();
            action.LoadPlainValue(actionPlainValue);
            return [PledgeCollectionUpdater.DeleteAsync(action.AgentAddress.GetPledgeAddress())];
        }

        if (Regex.IsMatch(actionType, "^request_pledge[0-9]*$"))
        {
            var action = new RequestPledge();
            action.LoadPlainValue(actionPlainValue);
            return
            [
                PledgeCollectionUpdater.UpsertAsync(
                    action.AgentAddress.GetPledgeAddress(),
                    signer,
                    false,
                    action.RefillMead)
            ];
        }

        return [];
    }
}
