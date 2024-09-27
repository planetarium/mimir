using System.Collections.Immutable;
using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;

public class CurrencySerializer : StructSerializerBase<Currency>
{
    private static class ElementNames
    {
        public const string Ticker = "Ticker";
        public const string DecimalPlaces = "DecimalPlaces";
        public const string Minters = "Minters";
        public const string TotalSupplyTrackable = "TotalSupplyTrackable";
        public const string MaximumSupplyMajor = "MaximumSupplyMajor";
        public const string MaximumSupplyMinor = "MaximumSupplyMinor";
    }

    public static readonly CurrencySerializer Instance = new();

    public static Currency Deserialize(BsonDocument doc)
    {
        var ticker = doc[ElementNames.Ticker].AsString;
        var decimalPlaces = (byte)doc[ElementNames.DecimalPlaces].AsInt32;
        var minters = doc.TryGetValue(ElementNames.Minters, out var mintersBsonValue)
            ? mintersBsonValue.AsBsonArray
                .Select(bsonValue => new Address(bsonValue.AsString))
                .ToImmutableHashSet()
            : null;
        var totalSupplyTrackable = doc[ElementNames.TotalSupplyTrackable].AsBoolean;
        var maximumSupplyMajor = doc.TryGetValue(ElementNames.MaximumSupplyMajor, out var maximumSupplyMajorBsonValue)
            ? BigInteger.Parse(maximumSupplyMajorBsonValue.AsString)
            : (BigInteger?)null;
        var maximumSupplyMinor = doc.TryGetValue(ElementNames.MaximumSupplyMinor, out var maximumSupplyMinorBsonValue)
            ? BigInteger.Parse(maximumSupplyMinorBsonValue.AsString)
            : (BigInteger?)null;
        if (totalSupplyTrackable)
        {
            (BigInteger, BigInteger)? maximumSupply = maximumSupplyMajor.HasValue || maximumSupplyMinor.HasValue
                ? (maximumSupplyMajor ?? BigInteger.Zero, maximumSupplyMinor ?? BigInteger.Zero)
                : null;
            return maximumSupply.HasValue
                ? Currency.Capped(
                    ticker,
                    decimalPlaces,
                    maximumSupply.Value,
                    minters)
                : Currency.Uncapped(ticker, decimalPlaces, minters);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Currency.Legacy(ticker, decimalPlaces, minters);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override Currency Deserialize(
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
    //     Currency value)
    // {
    //     var writer = context.Writer;
    //     writer.WriteStartDocument();
    //     writer.WriteName(ElementNames.Ticker);
    //     writer.WriteString(value.Ticker);
    //     writer.WriteName(ElementNames.DecimalPlaces);
    //     _decimalPlacesInfo.Serializer.Serialize(context, value.DecimalPlaces);
    //     if (value.Minters is not null)
    //     {
    //         writer.WriteName(ElementNames.Minters);
    //         _mintersInfo.Serializer.Serialize(context, value.Minters);
    //     }
    //
    //     writer.WriteName(ElementNames.TotalSupplyTrackable);
    //     writer.WriteBoolean(value.TotalSupplyTrackable);
    //     if (value.MaximumSupply.HasValue)
    //     {
    //         writer.WriteName(ElementNames.MaximumSupplyMajor);
    //         _maximumSupplyMajorInfo.Serializer.Serialize(context, value.MaximumSupply.Value.MajorUnit);
    //         writer.WriteName(ElementNames.MaximumSupplyMinor);
    //         _maximumSupplyMinorInfo.Serializer.Serialize(context, value.MaximumSupply.Value.MinorUnit);
    //     }
    //
    //     writer.WriteEndDocument();
    // }
}
