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
        if (!doc.TryGetValue("ItemType", out var itemTypeValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        if (!doc.TryGetValue("ItemSubType", out var itemSubTypeValue))
        {
            throw new BsonSerializationException("Missing itemSubTypeValue in document.");
        }

        var itemType = Enum.Parse<Nekoyume.Model.Item.ItemType>(itemTypeValue.AsString);
        var itemSubType = Enum.Parse<Nekoyume.Model.Item.ItemSubType>(itemSubTypeValue.AsString);
        switch (itemType)
        {
            case Nekoyume.Model.Item.ItemType.Equipment:
                switch (itemSubType)
                {
                    case Nekoyume.Model.Item.ItemSubType.Armor:
                        return ArmorSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Aura:
                        return AuraSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Belt:
                        return BeltSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Grimoire:
                        return GrimoireSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Necklace:
                        return NecklaceSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Ring:
                        return RingSerializer.Deserialize(doc);
                    case Nekoyume.Model.Item.ItemSubType.Weapon:
                        return WeaponSerializer.Deserialize(doc);
                }

                break;
        }

        throw new BsonSerializationException($"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}");
    }

    public static T Deserialize<T>(BsonDocument doc) where T : Equipment, new() => new T
    {
        Id = doc["Id"].AsInt32,
        Grade = doc["Grade"].AsInt32,
        ItemType = Enum.Parse<Nekoyume.Model.Item.ItemType>(doc["ItemType"].AsString),
        ItemSubType = Enum.Parse<Nekoyume.Model.Item.ItemSubType>(doc["ItemSubType"].AsString),
        ElementalType = Enum.Parse<Nekoyume.Model.Elemental.ElementalType>(doc["ElementalType"].AsString),
        ItemId = doc["ItemId"].AsGuid,
        StatsMap = StatMapSerializer.Deserialize(doc["StatMap"].AsBsonDocument),
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
