using Lib9c.Models.AttachmentActionResults;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;

public class Buy7BuyerResultSerializer : ClassSerializerBase<Buy7BuyerResult>
{
    public static readonly Buy7BuyerResultSerializer Instance = new();

    public static Buy7BuyerResult Deserialize(BsonDocument doc) => new()
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
        ShopItem = ShopItemSerializer.Deserialize(doc["ShopItem"].AsBsonDocument),
        Id = Guid.Parse(doc["Id"].AsString),
    };

    public override Buy7BuyerResult Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Buy7BuyerResult value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
