using Lib9c.Models.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class AuraSerializer : ClassSerializerBase<Aura>
{
    public static readonly AuraSerializer Instance = new();

    public static Aura Deserialize(BsonDocument doc) => EquipmentSerializer.Deserialize<Aura>(doc);

    public override Aura Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Aura value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
