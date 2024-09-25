using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class CostumeSerializer : ClassSerializerBase<Costume>
{
    public static readonly CostumeSerializer Instance = new();

    public static Costume Deserialize(BsonDocument doc) => new()
    {
        Id = doc["Id"].AsInt32,
        Grade = doc["Grade"].AsInt32,
        ItemType = (Nekoyume.Model.Item.ItemType)doc["ItemType"].AsInt32,
        ItemSubType = (Nekoyume.Model.Item.ItemSubType)doc["ItemSubType"].AsInt32,
        ElementalType = (Nekoyume.Model.Elemental.ElementalType)doc["ElementalType"].AsInt32,
        Equipped = doc["Equipped"].AsBoolean,
        SpineResourcePath = doc["SpineResourcePath"].AsString,
        ItemId = doc["ItemId"].AsGuid,
        RequiredBlockIndex = doc["RequiredBlockIndex"].ToLong(),
    };

    public override Costume Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Costume value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
