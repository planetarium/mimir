using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class ConsumableSerializer : ClassSerializerBase<Consumable>
{
    public static readonly ConsumableSerializer Instance = new();

    public static Consumable Deserialize(BsonDocument doc) => new()
    {
        Id = doc["Id"].AsInt32,
        Grade = doc["Grade"].AsInt32,
        ItemType = (Nekoyume.Model.Item.ItemType)doc["ItemType"].AsInt32,
        ItemSubType = (Nekoyume.Model.Item.ItemSubType)doc["ItemSubType"].AsInt32,
        ElementalType = (Nekoyume.Model.Elemental.ElementalType)doc["ElementalType"].AsInt32,
        ItemId = doc["ItemId"].AsGuid,
        StatsMap = StatMapSerializer.Deserialize(doc["StatMap"].AsBsonDocument),
        Skills = doc["Skills"].AsBsonArray
            .Select(skill => SkillSerializer.Deserialize(skill.AsBsonDocument))
            .ToList(),
        BuffSkills = doc["BuffSkills"].AsBsonArray
            .Select(skill => SkillSerializer.Deserialize(skill.AsBsonDocument))
            .ToList(),
        RequiredBlockIndex = doc["RequiredBlockIndex"].ToLong(),
        Stats = doc["Stats"].AsBsonArray
            .Select(stat => DecimalStatSerializer.Deserialize(stat.AsBsonDocument))
            .ToList(),
    };

    public override Consumable Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Consumable value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
