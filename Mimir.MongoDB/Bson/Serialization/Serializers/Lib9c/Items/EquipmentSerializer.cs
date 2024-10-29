using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class EquipmentSerializer : ClassSerializerBase<Equipment>
{
    public static readonly EquipmentSerializer Instance = new();

    public static Equipment Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("ItemType", out var itemTypeBsonValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        if (!doc.TryGetValue("ItemSubType", out var itemSubTypeBsonValue))
        {
            throw new BsonSerializationException("Missing itemSubTypeValue in document.");
        }

        var itemType = itemTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemType>();
        var itemSubType = itemSubTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemSubType>();
        return itemType switch
        {
            Nekoyume.Model.Item.ItemType.Equipment => itemSubType switch
            {
                Nekoyume.Model.Item.ItemSubType.Armor => ArmorSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Aura => AuraSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Belt => BeltSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Grimoire => GrimoireSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Necklace => NecklaceSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Ring => RingSerializer.Deserialize(doc),
                Nekoyume.Model.Item.ItemSubType.Weapon => WeaponSerializer.Deserialize(doc),
                _ => throw new BsonSerializationException(
                    $"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}"),
            },
            _ => throw new BsonSerializationException(
                $"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}"),
        };
    }

    public static T Deserialize<T>(BsonDocument doc) where T : Equipment, new() => new T
    {
        Id = doc["Id"].AsInt32,
        Grade = doc["Grade"].AsInt32,
        ItemType = doc["ItemType"].ToEnum<Nekoyume.Model.Item.ItemType>(),
        ItemSubType = doc["ItemSubType"].ToEnum<Nekoyume.Model.Item.ItemSubType>(),
        ElementalType = doc["ElementalType"].ToEnum<Nekoyume.Model.Elemental.ElementalType>(),
        ItemId = Guid.Parse(doc["ItemId"].AsString),
        StatsMap = StatMapSerializer.Deserialize(doc["StatsMap"].AsBsonDocument),
        Skills = doc["Skills"].AsBsonArray
            .Select(skill => SkillSerializer.Deserialize(skill.AsBsonDocument))
            .ToList(),
        BuffSkills = doc["BuffSkills"].AsBsonArray
            .Select(skill => SkillSerializer.Deserialize(skill.AsBsonDocument))
            .ToList(),
        RequiredBlockIndex = doc["RequiredBlockIndex"].ToLong(),
        Equipped = doc["Equipped"].AsBoolean,
        Level = doc["Level"].AsInt32,
        Exp = doc["Exp"].ToLong(),
        Stat = DecimalStatSerializer.Deserialize(doc["Stat"].AsBsonDocument),
        SetId = doc["SetId"].AsInt32,
        SpineResourcePath = doc["SpineResourcePath"].AsString,
        OptionCountFromCombination = doc["OptionCountFromCombination"].AsInt32,
        MadeWithMimisbrunnrRecipe = doc["MadeWithMimisbrunnrRecipe"].AsBoolean,
    };

    public override Equipment Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Equipment value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
