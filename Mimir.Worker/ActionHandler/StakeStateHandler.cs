using Bencodex.Types;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Initializer.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class StakeStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseActionHandler<StakeDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^stake[0-9]*$",
        Log.ForContext<StakeStateHandler>(),
        stateGetterService
    )
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        const string amountKey = "am";
        if (actionPlainValueInternal is not Dictionary dictionary)
        {
            // Skip when it cannot understand the actionPlainValueInternal.
            return [];
        }

        return await StakeCollectionUpdater.UpdateAsync(
            StateService,
            blockIndex,
            signer,
            (Integer)dictionary[amountKey],
            stoppingToken
        );
    }
}
