using Lib9c.Models.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class CostumeSerializer : ClassSerializerBase<Costume>
{
    public static readonly CostumeSerializer Instance = new();

    public static Costume Deserialize(BsonDocument doc) => new()
    {
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
