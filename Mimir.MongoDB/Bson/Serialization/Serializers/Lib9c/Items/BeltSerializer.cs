using Lib9c.Models.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class BeltSerializer : ClassSerializerBase<Belt>
{
    public static readonly BeltSerializer Instance = new();

    public static Belt Deserialize(BsonDocument doc) => EquipmentSerializer.Deserialize<Belt>(doc);

    public override Belt Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Belt value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
