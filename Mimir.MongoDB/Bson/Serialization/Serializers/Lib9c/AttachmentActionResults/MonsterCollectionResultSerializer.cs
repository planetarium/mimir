using Lib9c.Models.AttachmentActionResults;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Sheets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;

public class MonsterCollectionResultSerializer : ClassSerializerBase<MonsterCollectionResult>
{
    public static readonly MonsterCollectionResultSerializer Instance = new();

    public static MonsterCollectionResult Deserialize(BsonDocument doc) => new()
    {
        TypeId = doc["TypeId"].AsString,
        ItemUsable = doc.TryGetValue("ItemUsable", out var itemUsableBsonValue)
            ? ItemUsableSerializer.Deserialize(itemUsableBsonValue.AsBsonDocument)
            : null,
        Costume = doc.TryGetValue("Costume", out var costumeBsonValue)
            ? CostumeSerializer.Deserialize(costumeBsonValue.AsBsonDocument)
            : null,
        TradableFungibleItem = doc.TryGetValue("TradableFungibleItem", out var tradableFungibleItemBsonValue)
            ? TradableMaterialSerializer.Deserialize(tradableFungibleItemBsonValue.AsBsonDocument)
            : null,
        TradableFungibleItemCount = doc["TradableFungibleItemCount"].AsInt32,
        Id = Guid.Parse(doc["Id"].AsString),
        AvatarAddress = new Address(doc["AvatarAddress"].AsString),
        Rewards = doc["Rewards"].AsBsonArray
            .Select(e => MonsterCollectionRewardSheetRewardInfoSerializer.Deserialize(e.AsBsonDocument))
            .ToList(),
    };

    public override MonsterCollectionResult Deserialize(BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MonsterCollectionResult value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
