using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Skills;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class RingSerializer : ClassSerializerBase<Ring>
{
    public static readonly RingSerializer Instance = new();

    public static Ring Deserialize(BsonDocument doc) => EquipmentSerializer.Deserialize<Ring>(doc);

    public override Ring Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Ring value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
