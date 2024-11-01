using Lib9c.Models.Stats;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;

public class DecimalStatSerializer : ClassSerializerBase<DecimalStat>
{
    public static readonly DecimalStatSerializer Instance = new();

    public static DecimalStat Deserialize(BsonDocument doc) => new()
    {
        StatType = doc["StatType"].ToEnum<Nekoyume.Model.Stat.StatType>(),
        BaseValue = doc["BaseValue"].ToDecimal(),
        AdditionalValue = doc["AdditionalValue"].ToDecimal(),
    };

    public override DecimalStat Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DecimalStat value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
