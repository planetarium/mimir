using Lib9c.Models.Skills;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;

public class SkillSerializer : ClassSerializerBase<Skill>
{
    public static readonly SkillSerializer Instance = new();

    public static Skill Deserialize(BsonDocument doc) => new()
    {
        SkillRow = SkillSheetRowSerializer.Deserialize(doc["SkillRow"].AsBsonDocument),
        Power = doc["Power"].ToLong(),
        Chance = doc["Chance"].AsInt32,
        StatPowerRatio = doc["StatPowerRatio"].AsInt32,
        ReferencedStatType = doc["ReferencedStatType"].ToEnum<Nekoyume.Model.Stat.StatType>(),
    };

    public override Skill Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Skill value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
