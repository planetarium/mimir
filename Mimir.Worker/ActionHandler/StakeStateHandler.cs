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

public class StakeStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler<StakeDocument>(stateService, store, "^stake[0-9]*$", Log.ForContext<StakeStateHandler>())
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
        var action = new Stake();
        action.LoadPlainValue(actionPlainValue);
        return await StakeCollectionUpdater.UpdateAsync(
            StateService,
            signer,
            stoppingToken);
    }
}
