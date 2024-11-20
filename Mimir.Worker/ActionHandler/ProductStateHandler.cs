using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Market;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ProductStateHandler(
    IStateService stateService,
    MongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager
)
    : BaseActionHandler<ProductDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^register_product[0-9]*$|^cancel_product_registration[0-9]*$|^buy_product[0-9]*$|^re_register_product[0-9]*$",
        Log.ForContext<ProductStateHandler>()
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
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind
            );
        }

        var avatarAddresses = GetAvatarAddresses(actionType, actionValues);
        var ops = new List<WriteModel<BsonDocument>>();
        foreach (var avatarAddress in avatarAddresses)
        {
            var productsStateAddress = Nekoyume.Model.Market.ProductsState.DeriveAddress(
                avatarAddress
            );
            var productsState = await StateGetter.GetProductsState(avatarAddress, stoppingToken);
            ops.AddRange(
                await SyncWrappedProductsStateAsync(
                    blockIndex,
                    avatarAddress,
                    productsStateAddress,
                    productsState,
                    session,
                    stoppingToken
                )
            );
        }

        return ops;
    }

    private static List<Address> GetAvatarAddresses(string actionType, Dictionary actionValues)
    {
        var avatarAddresses = new List<Address>();

        if (Regex.IsMatch(actionType, "^buy_product[0-9]*$"))
        {
            var serialized = (List)actionValues["p"];
            var productInfos = serialized
                .Cast<List>()
                .Select(Nekoyume.Model.Market.ProductFactory.DeserializeProductInfo)
                .ToList();
            foreach (var productInfo in productInfos)
            {
                avatarAddresses.Add(productInfo.AvatarAddress);
            }
        }
        else
        {
            avatarAddresses.Add(new Address(actionValues["a"]));
        }

        return avatarAddresses;
    }

    public async Task<IEnumerable<WriteModel<BsonDocument>>> SyncWrappedProductsStateAsync(
        long blockIndex,
        Address avatarAddress,
        Address productsStateAddress,
        ProductsState productsState,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var productIds = productsState.ProductIds.Select(id => Guid.Parse(id.ToString())).ToList();
        var existingProductIds = await GetExistingProductIds(productsStateAddress);
        var productsToAdd = productIds.Except(existingProductIds).ToList();
        var productsToRemove = existingProductIds.Except(productIds).ToList();

        return (
            await AddNewProductsAsync(
                blockIndex,
                avatarAddress,
                productsStateAddress,
                productsToAdd,
                session,
                stoppingToken
            )
        ).Concat(RemoveOldProducts(productsToRemove));
    }

    private async Task<List<Guid>> GetExistingProductIds(Address productsStateAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq(
            "Object.ProductsStateAddress",
            productsStateAddress.ToHex()
        );
        var projection = Builders<BsonDocument>.Projection.Include("Object.ProductId");
        var documents = await Store
            .GetCollection<ProductDocument>()
            .Find<BsonDocument>(filter)
            .Project(projection)
            .ToListAsync();
        return documents is null
            ? []
            : documents.Select(doc => Guid.Parse(doc["Object"]["ProductId"].AsString)).ToList();
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> AddNewProductsAsync(
        long blockIndex,
        Address avatarAddress,
        Address productsStateAddress,
        List<Guid> productsToAdd,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var documents = new List<MimirBsonDocument>();
        foreach (var productId in productsToAdd)
        {
            try
            {
                var product = await StateGetter.GetProductState(productId, stoppingToken);
                var document = CreateProductDocumentAsync(
                    blockIndex,
                    avatarAddress,
                    productsStateAddress,
                    product
                );
                documents.Add(document);
            }
            catch (StateIsNullException e)
            {
                Logger.Fatal(
                    e,
                    "{AvatarAddress} Product[{ProductID}] is exists but state is `Bencodex Null`",
                    avatarAddress,
                    productId
                );
            }
        }

        return documents.Select(doc => doc.ToUpdateOneModel());
    }

    private ProductDocument CreateProductDocumentAsync(
        long blockIndex,
        Address avatarAddress,
        Address productsStateAddress,
        Product product
    )
    {
        var productAddress = Nekoyume.Model.Market.Product.DeriveAddress(product.ProductId);
        switch (product)
        {
            case ItemProduct itemProduct:
            {
                var unitPrice = CalculateUnitPrice(itemProduct);
                // var combatPoint = await CalculateCombatPointAsync(itemProduct);
                // var (crystal, crystalPerPrice) = await CalculateCrystalMetricsAsync(itemProduct);

                // return new ProductDocument(
                //     productAddress,
                //     avatarAddress,
                //     productsStateAddress,
                //     product,
                //     unitPrice,
                //     combatPoint,
                //     crystal,
                //     crystalPerPrice
                // );
                return new ProductDocument(
                    blockIndex,
                    productAddress,
                    avatarAddress,
                    productsStateAddress,
                    product,
                    unitPrice,
                    null,
                    null,
                    null
                );
            }
            case FavProduct favProduct:
            {
                var unitPrice = CalculateUnitPrice(favProduct);

                return new ProductDocument(
                    blockIndex,
                    productAddress,
                    avatarAddress,
                    productsStateAddress,
                    product,
                    unitPrice,
                    null,
                    null,
                    null
                );
            }
            default:
                return new ProductDocument(
                    blockIndex,
                    productAddress,
                    avatarAddress,
                    productsStateAddress,
                    product
                );
        }
    }

    private static decimal CalculateUnitPrice(ItemProduct itemProduct)
    {
        return decimal.Parse(itemProduct.Price.GetQuantityString()) / itemProduct.ItemCount;
    }

    private static decimal CalculateUnitPrice(FavProduct favProduct)
    {
        return decimal.Parse(favProduct.Price.GetQuantityString())
            / decimal.Parse(favProduct.Asset.GetQuantityString());
    }

    // private async Task<int?> CalculateCombatPointAsync(ItemProduct itemProduct)
    // {
    //     try
    //     {
    //         var costumeStatSheet = await Store.GetSheetAsync<CostumeStatSheet>();

    //         if (costumeStatSheet != null)
    //         {
    //             int? cp = itemProduct.TradableItem switch
    //             {
    //                 ItemUsable itemUsable
    //                     => CPHelper.GetCP(
    //                         (Nekoyume.Model.Item.ItemUsable)
    //                             Nekoyume.Model.Item.ItemFactory.Deserialize(
    //                                 (Dictionary)itemUsable.Bencoded
    //                             )
    //                     ),
    //                 Costume costume => CPHelper.GetCP(new Nekoyume.Model.Item.Costume((Dictionary)costume.Bencoded), costumeStatSheet),
    //                 _ => null
    //             };
    //             return cp;
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.Error(
    //             $"Error calculating combat point for itemProduct {itemProduct.ProductId}: {ex.Message}"
    //         );
    //     }

    //     return null;
    // }

    // private async Task<(int? crystal, int? crystalPerPrice)> CalculateCrystalMetricsAsync(
    //     ItemProduct itemProduct
    // )
    // {
    //     try
    //     {
    //         var crystalEquipmentGrindingSheet =
    //             await Store.GetSheetAsync<CrystalEquipmentGrindingSheet>();
    //         var crystalMonsterCollectionMultiplierSheet =
    //             await Store.GetSheetAsync<CrystalMonsterCollectionMultiplierSheet>();

    //         if (
    //             crystalEquipmentGrindingSheet != null
    //             && crystalMonsterCollectionMultiplierSheet != null
    //             && itemProduct.TradableItem is Equipment equipment
    //         )
    //         {
    //             var rawCrystal = CrystalCalculator.CalculateCrystal(
    //                 [equipment],
    //                 false,
    //                 crystalEquipmentGrindingSheet,
    //                 crystalMonsterCollectionMultiplierSheet,
    //                 0
    //             );

    //             int crystal = (int)rawCrystal.MajorUnit;
    //             int crystalPerPrice = (int)
    //                 rawCrystal.DivRem(itemProduct.Price.MajorUnit).Quotient.MajorUnit;

    //             return (crystal, crystalPerPrice);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.Error(
    //             $"Error calculating crystal metrics for itemProduct {itemProduct.ProductId}: {ex.Message}"
    //         );
    //     }

    //     return (null, null);
    // }

    private IEnumerable<WriteModel<BsonDocument>> RemoveOldProducts(List<Guid> productsToRemove)
    {
        var ops = new List<WriteModel<BsonDocument>>();
        foreach (var productId in productsToRemove)
        {
            var productFilter = Builders<BsonDocument>.Filter.Eq(
                "Object.TradableItem.TradableId",
                productId.ToString()
            );
            ops.Add(new DeleteOneModel<BsonDocument>(productFilter));
        }

        return ops;
    }
}
