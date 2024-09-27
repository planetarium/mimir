using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nekoyume.TableData;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;

public class MonsterCollectionRewardSheetRewardInfoSerializer :
    ClassSerializerBase<MonsterCollectionRewardSheet.RewardInfo>
{
    public static readonly MonsterCollectionRewardSheetRewardInfoSerializer Instance = new();

    public static MonsterCollectionRewardSheet.RewardInfo Deserialize(BsonDocument doc)
    {
        var fields = new[]
        {
            doc["ItemId"].AsInt32.ToString(),
            doc["Quantity"].AsInt32.ToString(),
        };
        return new MonsterCollectionRewardSheet.RewardInfo(fields);
    }

    public override MonsterCollectionRewardSheet.RewardInfo Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MonsterCollectionRewardSheet.RewardInfo value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
