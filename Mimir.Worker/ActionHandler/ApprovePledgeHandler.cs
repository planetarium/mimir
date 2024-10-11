using Bencodex.Types;
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
        Log.ForContext<ApprovePledgeHandler>())
{
    private static readonly ApprovePledge Action = new();

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
        await PledgeCollectionUpdater.ApproveAsync(Store, Action.PatronAddress, session, stoppingToken);
        return true;
    }
}
