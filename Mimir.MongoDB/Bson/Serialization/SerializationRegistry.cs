using System.Numerics;
using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.Skills;
using Lib9c.Models.States;
using Lib9c.Models.Stats;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Market;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.States;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
using MongoDB.Bson;
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
        BsonSerializer.TryRegisterSerializer(typeof(BigInteger), BigIntegerSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(
            typeof(Guid),
            GuidSerializer.StandardInstance.WithRepresentation(BsonType.String));

        // Libplanet
        BsonSerializer.TryRegisterSerializer(typeof(Address), AddressSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Currency), CurrencySerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(FungibleAssetValue), FungibleAssetValueSerializer.Instance);

        // Lib9c.Models.AttachmentActionResults
        BsonSerializer.TryRegisterSerializer(typeof(AttachmentActionResult), AttachmentActionResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Buy7BuyerResult), Buy7BuyerResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Buy7SellerResult), Buy7SellerResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(
            typeof(CombinationConsumable5Result),
            CombinationConsumable5ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(DailyReward2Result), DailyReward2ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemEnhancement7Result), ItemEnhancement7ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemEnhancement9Result), ItemEnhancement9ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemEnhancement11Result), ItemEnhancement11ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemEnhancement12Result), ItemEnhancement12ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemEnhancement13Result), ItemEnhancement13ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(MonsterCollectionResult), MonsterCollectionResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(RapidCombination0Result), RapidCombination0ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(RapidCombination5Result), RapidCombination5ResultSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(SellCancellationResult), SellCancellationResultSerializer.Instance);

        // Lib9c.Models.Items
        BsonSerializer.TryRegisterSerializer(typeof(Armor), ArmorSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Aura), AuraSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Belt), BeltSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Consumable), ConsumableSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Costume), CostumeSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Equipment), EquipmentSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Grimoire), GrimoireSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemBase), ItemBaseSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemUsable), ItemUsableSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Material), MaterialSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Necklace), NecklaceSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Ring), RingSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ShopItem), ShopItemSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(TradableMaterial), TradableMaterialSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(Weapon), WeaponSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(
            typeof(Dictionary<Material, int>),
            new DictionaryInterfaceImplementerSerializer<Dictionary<Material, int>>()
                .WithKeySerializer(MaterialSerializer.Instance));

        // Lib9c.Models.Market
        BsonSerializer.TryRegisterSerializer(typeof(Product), ProductSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(FavProduct), FavProductSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(ItemProduct), ItemProductSerializer.Instance);

        // Lib9c.Models.Skills
        BsonSerializer.TryRegisterSerializer(typeof(Skill), SkillSerializer.Instance);

        // Lib9c.Models.States
        BsonSerializer.TryRegisterSerializer(typeof(CombinationSlotState), CombinationSlotStateSerializer.Instance);

        // Lib9c.Models.Stats
        BsonSerializer.TryRegisterSerializer(typeof(DecimalStat), DecimalStatSerializer.Instance);
        BsonSerializer.TryRegisterSerializer(typeof(StatMap), StatMapSerializer.Instance);

        // Nekoyume.TableData
        BsonSerializer.TryRegisterSerializer(typeof(SkillSheet.Row), SkillSheetRowSerializer.Instance);
    }
}
