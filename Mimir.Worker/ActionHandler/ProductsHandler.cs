using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Battle;
using Nekoyume.Helper;
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ProductsHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^register_product[0-9]*$|^cancel_product_registration[0-9]*$|^buy_product[0-9]*$|^re_register_product[0-9]*$",
        Log.ForContext<ProductsHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        string actionType,
        long processBlockIndex,
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

        Logger.Information(
            "Handle products, product, addresses: {AvatarAddresses}",
            string.Join(", ", avatarAddresses)
        );

        foreach (var avatarAddress in avatarAddresses)
        {
            var productsStateAddress = Nekoyume.Model.Market.ProductsState.DeriveAddress(
                avatarAddress
            );
            var productsState = await StateGetter.GetProductsState(avatarAddress);
            if (productsState is null)
            {
                continue;
            }

            await SyncWrappedProductsStateAsync(
                avatarAddress,
                productsStateAddress,
                productsState,
                session,
                stoppingToken
            );
        }

        return true;
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

    public async Task SyncWrappedProductsStateAsync(
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

        await AddNewProductsAsync(avatarAddress, productsStateAddress, productsToAdd, session);
        await RemoveOldProductsAsync(productsToRemove);

        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ProductsStateDocument>(),
            [new ProductsStateDocument(productsStateAddress, productsState, avatarAddress)],
            session,
            stoppingToken
        );
    }

    private async Task<List<Guid>> GetExistingProductIds(Address productsStateAddress)
    {
        var existingState = await Store.GetProductsStateByAddress(productsStateAddress);
        return existingState == null
            ? new List<Guid>()
            : existingState["Object"]
                ["ProductIds"]
                .AsBsonArray.Select(p => Guid.Parse(p.AsString))
                .ToList();
    }

    private async Task AddNewProductsAsync(
        Address avatarAddress,
        Address productsStateAddress,
        List<Guid> productsToAdd,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        List<MimirBsonDocument> documents = new List<MimirBsonDocument>();

        foreach (var productId in productsToAdd)
        {
            var product = await StateGetter.GetProductState(productId);
            var document = CreateProductDocumentAsync(avatarAddress, productsStateAddress, product);
            documents.Add(document);
        }

        if (documents.Count > 0)
        {
            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<ProductDocument>(),
                documents,
                session,
                stoppingToken
            );
        }
    }

    private ProductDocument CreateProductDocumentAsync(
        Address avatarAddress,
        Address productsStateAddress,
        Product product
    )
    {
        var productAddress = Nekoyume.Model.Market.Product.DeriveAddress(product.ProductId);

        if (product is ItemProduct itemProduct)
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
        else
        {
            return new ProductDocument(
                productAddress,
                avatarAddress,
                productsStateAddress,
                product
            );
        }
    }

    private decimal CalculateUnitPrice(ItemProduct itemProduct)
    {
        return decimal.Parse(itemProduct.Price.GetQuantityString()) / itemProduct.ItemCount;
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

    private async Task RemoveOldProductsAsync(List<Guid> productsToRemove)
    {
        foreach (var productId in productsToRemove)
        {
            await Store.RemoveProduct(productId);
        }
    }
}
