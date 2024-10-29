using Lib9c.Models.Market;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Market;

public class ProductSerializer : ClassSerializerBase<Product>
{
    public static readonly ProductSerializer Instance = new();

    public static Product Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("ProductType", out var productTypeValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        var productType = productTypeValue.ToEnum<Nekoyume.Model.Market.ProductType>();
        return productType switch
        {
            Nekoyume.Model.Market.ProductType.Fungible or
                Nekoyume.Model.Market.ProductType.NonFungible => ItemProductSerializer.Deserialize(doc),
            Nekoyume.Model.Market.ProductType.FungibleAssetValue => FavProductSerializer.Deserialize(doc),
            _ => throw new BsonSerializationException($"Unsupported ProductType: {productType}"),
        };
    }

    public override Product Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
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
