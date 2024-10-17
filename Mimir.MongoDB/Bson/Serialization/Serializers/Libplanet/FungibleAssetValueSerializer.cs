using System.Numerics;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;

public class FungibleAssetValueSerializer : StructSerializerBase<FungibleAssetValue>
{
    private static class ElementNames
    {
        public const string Currency = "Currency";
        public const string RawValue = "RawValue";
    }

    public static readonly FungibleAssetValueSerializer Instance = new();

    public static FungibleAssetValue Deserialize(BsonDocument doc)
    {
        var currency = CurrencySerializer.Deserialize(doc[ElementNames.Currency].AsBsonDocument);
        var rawValue = doc.GetValueOrDefault(ElementNames.RawValue, bsonValue => BigInteger.Parse(bsonValue.AsString));
        return FungibleAssetValue.FromRawValue(currency, rawValue);
    }

    public override FungibleAssetValue Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(
    //     BsonSerializationContext context,
    //     BsonSerializationArgs args,
    //     FungibleAssetValue value)
    // {
    //     var writer = context.Writer;
    //     writer.WriteStartDocument();
    //     writer.WriteName(ElementNames.Currency);
    //     _currencyInfo.Serializer.Serialize(context, value.Currency);
    //     writer.WriteName(ElementNames.RawValue);
    //     _rawValueInfo.Serializer.Serialize(context, value.RawValue);
    //     writer.WriteEndDocument();
    // }
}
