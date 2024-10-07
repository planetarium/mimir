using Lib9c.Models.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class NecklaceSerializer : ClassSerializerBase<Necklace>
{
    public static readonly NecklaceSerializer Instance = new();

    public static Necklace Deserialize(BsonDocument doc) => EquipmentSerializer.Deserialize<Necklace>(doc);

    public override Necklace Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Necklace value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
