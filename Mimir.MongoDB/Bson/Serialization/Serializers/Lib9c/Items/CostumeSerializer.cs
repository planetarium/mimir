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
        ItemType = Enum.Parse<Nekoyume.Model.Item.ItemType>(doc["ItemType"].AsString),
        ItemSubType = Enum.Parse<Nekoyume.Model.Item.ItemSubType>(doc["ItemSubType"].AsString),
        ElementalType = Enum.Parse<Nekoyume.Model.Elemental.ElementalType>(doc["ElementalType"].AsString),
        Equipped = doc["Equipped"].AsBoolean,
        SpineResourcePath = doc["SpineResourcePath"].AsString,
        ItemId = Guid.Parse(doc["ItemId"].AsString),
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
