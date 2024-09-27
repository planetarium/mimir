using System.Numerics;
using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Items;
using Lib9c.Models.Skills;
using Lib9c.Models.States;
using Lib9c.Models.Stats;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.States;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson.Serialization;

public static class SerializationRegistry
{
    public static void Register()
    {
        RegisterClassMaps();
        RegisterSerializers();
    }

    private static void RegisterClassMaps()
    {
    }

    private static void RegisterSerializers()
    {
        // System
        BsonSerializer.RegisterSerializer(typeof(BigInteger), BigIntegerSerializer.Instance);

        // Libplanet
        BsonSerializer.RegisterSerializer(typeof(Address), AddressSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Currency), CurrencySerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(FungibleAssetValue), FungibleAssetValueSerializer.Instance);

        // Lib9c.Models.AttachmentActionResults
        BsonSerializer.RegisterSerializer(typeof(AttachmentActionResult), AttachmentActionResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Buy7BuyerResult), Buy7BuyerResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Buy7SellerResult), Buy7SellerResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(
            typeof(CombinationConsumable5Result),
            CombinationConsumable5ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(DailyReward2Result), DailyReward2ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemEnhancement7Result), ItemEnhancement7ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemEnhancement9Result), ItemEnhancement9ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemEnhancement11Result), ItemEnhancement11ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemEnhancement12Result), ItemEnhancement12ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemEnhancement13Result), ItemEnhancement13ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(MonsterCollectionResult), MonsterCollectionResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(RapidCombination0Result), RapidCombination0ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(RapidCombination5Result), RapidCombination5ResultSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(SellCancellationResult), SellCancellationResultSerializer.Instance);

        // Lib9c.Models.Items
        BsonSerializer.RegisterSerializer(typeof(Armor), ArmorSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Aura), AuraSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Belt), BeltSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Consumable), ConsumableSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Costume), CostumeSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Equipment), EquipmentSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Grimoire), GrimoireSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemBase), ItemBaseSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ItemUsable), ItemUsableSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Material), MaterialSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Necklace), NecklaceSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Ring), RingSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(ShopItem), ShopItemSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(TradableMaterial), TradableMaterialSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Weapon), WeaponSerializer.Instance);
        BsonSerializer.RegisterSerializer(
            typeof(Dictionary<Material, int>),
            new DictionaryInterfaceImplementerSerializer<Dictionary<Material, int>>()
                .WithKeySerializer(MaterialSerializer.Instance));

        // Lib9c.Models.Skills
        BsonSerializer.RegisterSerializer(typeof(Skill), SkillSerializer.Instance);

        // Lib9c.Models.States
        BsonSerializer.RegisterSerializer(typeof(CombinationSlotState), CombinationSlotStateSerializer.Instance);

        // Lib9c.Models.Stats
        BsonSerializer.RegisterSerializer(typeof(DecimalStat), DecimalStatSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(StatMap), StatMapSerializer.Instance);

        // Nekoyume.TableData
        BsonSerializer.RegisterSerializer(typeof(SkillSheet.Row), SkillSheetRowSerializer.Instance);
    }
}
