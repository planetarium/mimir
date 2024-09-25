using System.Numerics;
using Lib9c.Models.Items;
using Lib9c.Models.Skills;
using Lib9c.Models.Stats;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
using MongoDB.Bson.Serialization;
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
        BsonSerializer.RegisterSerializer(typeof(TradableMaterial), TradableMaterialSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Weapon), WeaponSerializer.Instance);

        // Lib9c.Models.Skills
        BsonSerializer.RegisterSerializer(typeof(Skill), SkillSerializer.Instance);

        // Lib9c.Models.Stats
        BsonSerializer.RegisterSerializer(typeof(DecimalStat), DecimalStatSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(StatMap), StatMapSerializer.Instance);

        // Nekoyume.TableData
        BsonSerializer.RegisterSerializer(typeof(SkillSheet.Row), SkillSheetRowSerializer.Instance);
    }
}
