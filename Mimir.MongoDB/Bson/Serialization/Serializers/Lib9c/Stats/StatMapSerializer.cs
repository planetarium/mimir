using Lib9c.Models.Stats;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Stats;

public class StatMapSerializer : ClassSerializerBase<StatMap>
{
    public static readonly StatMapSerializer Instance = new();

    public static StatMap Deserialize(BsonDocument doc)
    {
        var statMap = new StatMap
        {
            Value = new Dictionary<Nekoyume.Model.Stat.StatType, DecimalStat>(),
        };
        var allStatTypes = Enum.GetValues<Nekoyume.Model.Stat.StatType>();
        foreach (var targetStatType in allStatTypes)
        {
            if (!doc.TryGetValue(targetStatType.ToString(), out var targetStat))
            {
                continue;
            }

            var statDoc = targetStat.AsBsonDocument;
            statMap.Value[targetStatType] = DecimalStatSerializer.Deserialize(statDoc);
        }

        return statMap;
    }

    public override StatMap Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StatMap value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
