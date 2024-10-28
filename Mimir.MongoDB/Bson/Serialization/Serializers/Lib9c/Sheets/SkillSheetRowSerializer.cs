using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;

public class SkillSheetRowSerializer : ClassSerializerBase<SkillSheet.Row>
{
    public static readonly SkillSheetRowSerializer Instance = new();

    public static SkillSheet.Row Deserialize(BsonDocument doc)
    {
        var fields = new[]
        {
            doc["Id"].AsInt32.ToString(),
            doc["ElementalType"].ToEnum<Nekoyume.Model.Elemental.ElementalType>().ToString(),
            doc["SkillType"].ToEnum<Nekoyume.Model.Skill.SkillType>().ToString(),
            doc["SkillCategory"].ToEnum<Nekoyume.Model.Skill.SkillCategory>().ToString(),
            doc["SkillTargetType"].ToEnum<Nekoyume.Model.Skill.SkillTargetType>().ToString(),
            doc["HitCount"].AsInt32.ToString(),
            doc["Cooldown"].AsInt32.ToString(),
            doc["Combo"].AsBoolean.ToString(),
        };
        var row = new SkillSheet.Row();
        row.Set(fields);
        return row;
    }

    public override SkillSheet.Row Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SkillSheet.Row value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
