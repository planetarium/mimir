using Lib9c.Models.Market;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Market;

public class ItemProductSerializer : ClassSerializerBase<ItemProduct>
{
    public static readonly ProductSerializer Instance = new();

    public static ItemProduct Deserialize(BsonDocument doc) => new()
    {
        ProductId = Guid.Parse(doc["ProductId"].AsString),
        ProductType = doc["ProductType"].ToEnum<Nekoyume.Model.Market.ProductType>(),
        Price = FungibleAssetValueSerializer.Deserialize(doc["Price"].AsBsonDocument),
        RegisteredBlockIndex = doc["RegisteredBlockIndex"].ToLong(),
        SellerAvatarAddress = new Address(doc["SellerAvatarAddress"].AsString),
        SellerAgentAddress = new Address(doc["SellerAgentAddress"].AsString),
        TradableItem = ItemBaseSerializer.Deserialize(doc["TradableItem"].AsBsonDocument),
        ItemCount = doc["ItemCount"].AsInt32,
    };

    public override ItemProduct Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ItemBase value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
