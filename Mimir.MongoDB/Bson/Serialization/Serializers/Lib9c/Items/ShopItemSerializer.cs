using Lib9c.Models.Items;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class ShopItemSerializer : ClassSerializerBase<ShopItem>
{
    public static readonly ShopItemSerializer Instance = new();

    public static ShopItem Deserialize(BsonDocument doc) => new()
    {
        SellerAgentAddress = new Address(doc["SellerAgentAddress"].AsString),
        SellerAvatarAddress = new Address(doc["SellerAvatarAddress"].AsString),
        ProductId = Guid.Parse(doc["ProductId"].AsString),
        Price = FungibleAssetValueSerializer.Deserialize(doc["Price"].AsBsonDocument),
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
        ExpiredBlockIndex = doc["ExpiredBlockIndex"].ToLong(),
    };

    public override ShopItem Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ShopItem value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
