using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Nekoyume.Model.Market;
using Serilog;

namespace Mimir.Worker.Initializer;

public class MarketInitializer : BaseInitializer
{
    private StateGetter _stateGetter;

    public MarketInitializer(IStateService service, MongoDbService store)
        : base(service, store, Log.ForContext<MarketInitializer>())
    {
        _stateGetter = service.At();
    }

    public override async Task<bool> IsInitialized()
    {
        var collection = _store.GetCollection(
            CollectionNames.GetCollectionName<WrappedProductsState>()
        );
        var count = await collection.CountDocumentsAsync(new BsonDocument());

        return count > 0;
    }

    public override async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Start {Name} initializing", nameof(MarketInitializer));

        var marketState = await _stateGetter.GetMarketState();
        var productHandler = new ProductsHandler(_stateService, _store);

        _logger.Information(
            "Total avatarAddressCount: {Count} ",
            marketState.AvatarAddresses.Count()
        );

        foreach (var avatarAddress in marketState.AvatarAddresses)
        {
            _logger.Information(
                "Handle products for avatar, avatar: {AvatarAddress} ",
                avatarAddress
            );

            var productsStateAddress = ProductsState.DeriveAddress(avatarAddress);
            var productsState = await _stateGetter.GetProductsState(avatarAddress);
            await productHandler.SyncWrappedProductsStateAsync(
                avatarAddress,
                productsStateAddress,
                productsState
            );
        }
    }
}
