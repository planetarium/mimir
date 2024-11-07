using System.Diagnostics.CodeAnalysis;
using Lib9c.Models.Market;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Market;

public class ProductSerializer : ClassSerializerBase<Product>, IBsonDocumentSerializer
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

    public bool TryGetMemberSerializationInfo(string memberName, [UnscopedRef] out BsonSerializationInfo? serializationInfo)
    {
        switch (memberName)
        {
            case nameof(Product.Price):
            {
                serializationInfo = new BsonSerializationInfo(memberName, FungibleAssetValueSerializer.Instance, typeof(FungibleAssetValue));
                return true;
            }
            case nameof(Product.ProductType):
            {
                serializationInfo = new BsonSerializationInfo(memberName, new EnumSerializer<Nekoyume.Model.Market.ProductType>(BsonType.String), typeof(Nekoyume.Model.Market.ProductType));
                return true;
            }
            default:
            {
                serializationInfo = null;
                return false;
            }
        }
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ItemBase value)
    // {
    //     base.Serialize(context, args, value);
    // }
}