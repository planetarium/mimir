using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class StakeStateHandler(IStateService stateService, MongoDbService store, IHeadlessGQLClient headlessGqlClient, IInitializerManager initializerManager) :
    BaseActionHandler<StakeDocument>(stateService, store, headlessGqlClient, initializerManager, "^stake[0-9]*$", Log.ForContext<StakeStateHandler>())
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
        const string amountKey = "am";
        if (actionPlainValueInternal is not Dictionary dictionary)
        {
            // Skip when it cannot understand the actionPlainValueInternal.
            return [];
        }

        return await StakeCollectionUpdater.UpdateAsync(
            StateService,
            signer,
            (Integer)dictionary[amountKey],
            stoppingToken);
    }
}
