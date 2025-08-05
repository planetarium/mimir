using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
// using Mimir.Worker.ActionHandler;
// using Mimir.Shared.Constants;
// using Mimir.Shared.Services;
// using Mimir.Worker.Util;
// using MongoDB.Bson;
// using Serilog;
// using NCProductsState = Nekoyume.Model.Market.ProductsState;
// using ProductsState = Mimir.Worker.Models.ProductsState;

// namespace Mimir.Worker.Initializer;

// public class MarketInitializer : BaseInitializer
// {
//     private StateGetter _stateGetter;

//     public MarketInitializer(IStateService service, IMongoDbService store)
//         : base(service, store, Log.ForContext<MarketInitializer>())
//     {
//         _stateGetter = service.At();
//     }

//     public override async Task<bool> IsInitialized()
//     {
//         var collection = _store.GetCollection(CollectionNames.GetCollectionName<ProductsState>());
//         var count = await collection.CountDocumentsAsync(new BsonDocument());

//         return count > 0;
//     }

//     public override async Task RunAsync(CancellationToken stoppingToken)
//     {
//         _logger.Information("Start {Name} initializing", nameof(MarketInitializer));

//         var marketState = await _stateGetter.GetMarketState();
//         var productHandler = new ProductsHandler(_stateService, _store);

//         _logger.Information(
//             "Total avatarAddressCount: {Count} ",
//             marketState.AvatarAddresses.Count()
//         );

//         using (var session = await _store.GetMongoClient().StartSessionAsync())
//         {
//             session.StartTransaction();

//             foreach (var avatarAddress in marketState.AvatarAddresses)
//             {
//                 _logger.Information(
//                     "Handle products for avatar, avatar: {AvatarAddress} ",
//                     avatarAddress
//                 );

//                 var productsStateAddress = NCProductsState.DeriveAddress(avatarAddress);
//                 var productsState = await _stateGetter.GetProductsState(avatarAddress);
//                 await productHandler.SyncWrappedProductsStateAsync(
//                     session,
//                     avatarAddress,
//                     productsStateAddress,
//                     productsState
//                 );
//             }
//             session.CommitTransaction();
//         }
//     }
// }
