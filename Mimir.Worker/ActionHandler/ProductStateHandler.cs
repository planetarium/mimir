using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Battle;
using Nekoyume.Helper;
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ProductStateHandler(
    IStateService stateService,
    MongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IItemProductCalculationService itemProductCalculationService
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
            "ProductsStateAddress",
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
                var document = await CreateProductDocumentAsync(
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

    private async Task<ProductDocument> CreateProductDocumentAsync(
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
                
                int? crystal = null;
                int? crystalPerPrice = null;
                int? combatPoint = null;
                try
                {
                    (crystal, crystalPerPrice) = await itemProductCalculationService.CalculateCrystalMetricsAsync(itemProduct);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error calculating crystal metrics for itemProduct {ItemProductProductId}: {ExMessage}",
                        itemProduct.ProductId, ex.Message);
                }

                try
                {
                    combatPoint = await itemProductCalculationService.CalculateCombatPointAsync(itemProduct);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error calculating combat point for itemProduct {ItemProductProductId}: {ExMessage}",
                        itemProduct.ProductId, ex.Message);
                }
                
                return new ProductDocument(
                    blockIndex,
                    productAddress,
                    avatarAddress,
                    productsStateAddress,
                    product,
                    unitPrice,
                    combatPoint,
                    crystal,
                    crystalPerPrice
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
    
    private IEnumerable<WriteModel<BsonDocument>> RemoveOldProducts(List<Guid> productsToRemove)
    {
            var productFilter = Builders<BsonDocument>.Filter.In(
                "Object.ProductId",
                productsToRemove.Select(x=>x.ToString())
            );
            yield return new DeleteManyModel<BsonDocument>(productFilter);
    }
}
