using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using System;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Mimir.MongoDB.Services;
using Nekoyume.Battle;
using Nekoyume.Helper;
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;

namespace Mimir.Worker.Services;

public class ItemProductCalculationService : IItemProductCalculationService
{
    private readonly IMongoDbService _store;

    public ItemProductCalculationService(IMongoDbService store)
    {
        _store = store;
    }

    public async Task<long?> CalculateCombatPointAsync(ItemProduct itemProduct)
    {
        var costumeStatSheet = await _store.GetSheetAsync<CostumeStatSheet>();

        if (costumeStatSheet != null)
        {
            long? cp = itemProduct.TradableItem switch
            {
                ItemUsable itemUsable => CPHelper.GetCP(
                    (Nekoyume.Model.Item.ItemUsable)
                        Nekoyume.Model.Item.ItemFactory.Deserialize((Dictionary)itemUsable.Bencoded)
                ),
                Costume costume => CPHelper.GetCP(
                    new Nekoyume.Model.Item.Costume((Dictionary)costume.Bencoded),
                    costumeStatSheet
                ),
                _ => null,
            };
            return cp;
        }

        return null;
    }

    public async Task<(int? crystal, int? crystalPerPrice)> CalculateCrystalMetricsAsync(
        ItemProduct itemProduct
    )
    {
        var crystalEquipmentGrindingSheet =
            await _store.GetSheetAsync<CrystalEquipmentGrindingSheet>();
        var crystalMonsterCollectionMultiplierSheet =
            await _store.GetSheetAsync<CrystalMonsterCollectionMultiplierSheet>();

        if (
            crystalEquipmentGrindingSheet != null
            && crystalMonsterCollectionMultiplierSheet != null
            && itemProduct.TradableItem is Equipment equipment
        )
        {
            var rawCrystal = CrystalCalculator.CalculateCrystal(
                [equipment.ToNekoyumeEquipment()],
                false,
                crystalEquipmentGrindingSheet,
                crystalMonsterCollectionMultiplierSheet,
                0
            );

            int crystal = (int)rawCrystal.MajorUnit;
            int crystalPerPrice = (int)
                rawCrystal.DivRem(itemProduct.Price.MajorUnit).Quotient.MajorUnit;

            return (crystal, crystalPerPrice);
        }

        return (null, null);
    }
}
