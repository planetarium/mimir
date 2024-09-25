using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nekoyume.TableData;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;

public class SkillSheetRowSerializer : ClassSerializerBase<SkillSheet.Row>
{
    public static readonly SkillSheetRowSerializer Instance = new();

    public static SkillSheet.Row Deserialize(BsonDocument doc)
    {
        var jsonString = doc.ToJson();
        return JsonConvert.DeserializeObject<SkillSheet.Row>(jsonString) ??
               throw new JsonSerializationException(
                   "Failed to deserialize SkillSheet.Row from BSON document." +
                   $" jsonString: {jsonString}");
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
