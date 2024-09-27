using System.Collections.Immutable;
using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
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

    private static class Flags
    {
        public const long Ticker = 1L << 0;
        public const long DecimalPlaces = 1L << 1;
        public const long Minters = 1L << 2;
        public const long TotalSupplyTrackable = 1L << 3;
        public const long MaximumSupplyMajor = 1L << 4;
        public const long MaximumSupplyMinor = 1L << 5;
    }

    public static readonly CurrencySerializer Instance = new();

    private readonly SerializerHelper _helper = new(
        new SerializerHelper.Member(ElementNames.Ticker, Flags.Ticker),
        new SerializerHelper.Member(ElementNames.DecimalPlaces, Flags.DecimalPlaces),
        new SerializerHelper.Member(ElementNames.Minters, Flags.Minters),
        new SerializerHelper.Member(ElementNames.TotalSupplyTrackable, Flags.TotalSupplyTrackable),
        new SerializerHelper.Member(ElementNames.MaximumSupplyMajor, Flags.MaximumSupplyMajor),
        new SerializerHelper.Member(ElementNames.MaximumSupplyMinor, Flags.MaximumSupplyMinor));

    private readonly BsonSerializationInfo _decimalPlacesInfo = new(
        ElementNames.DecimalPlaces,
        new ByteSerializer(),
        typeof(byte));

    private readonly BsonSerializationInfo _mintersInfo = new(
        ElementNames.Minters,
        new ArraySerializer<AddressSerializer>(),
        typeof(IImmutableSet<Address>));

    private readonly BsonSerializationInfo _maximumSupplyMajorInfo = new(
        ElementNames.MaximumSupplyMajor,
        new NullableSerializer<BigInteger>(BigIntegerSerializer.Instance),
        typeof(BigInteger?));

    private readonly BsonSerializationInfo _maximumSupplyMinorInfo = new(
        ElementNames.MaximumSupplyMinor,
        new NullableSerializer<BigInteger>(BigIntegerSerializer.Instance),
        typeof(BigInteger?));

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
        var maximumSupplyMinor = doc.TryGetValue(ElementNames.MaximumSupplyMajor, out var maximumSupplyMinorBsonValue)
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
        var ticker = string.Empty;
        byte decimalPlaces = default;
        Address[]? minters = null;
        bool totalSupplyTrackable = default;
        BigInteger? maximumSupplyMajor = null;
        BigInteger? maximumSupplyMinor = null;
        _helper.DeserializeMembers(context, (_, flag) =>
        {
            switch (flag)
            {
                case Flags.Ticker:
                    ticker = context.Reader.ReadString();
                    break;
                case Flags.DecimalPlaces:
                    decimalPlaces = (byte)_decimalPlacesInfo.Serializer.Deserialize(context);
                    break;
                case Flags.Minters:
                    minters = (Address[]?)_mintersInfo.Serializer.Deserialize(context);
                    break;
                case Flags.TotalSupplyTrackable:
                    totalSupplyTrackable = context.Reader.ReadBoolean();
                    break;
                case Flags.MaximumSupplyMajor:
                    maximumSupplyMajor = (BigInteger?)_maximumSupplyMajorInfo.Serializer.Deserialize(context);
                    break;
                case Flags.MaximumSupplyMinor:
                    maximumSupplyMinor = (BigInteger?)_maximumSupplyMinorInfo.Serializer.Deserialize(context);
                    break;
            }
        });

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
                    minters?.ToImmutableHashSet())
                : Currency.Uncapped(ticker, decimalPlaces, minters?.ToImmutableHashSet());
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Currency.Legacy(ticker, decimalPlaces, minters?.ToImmutableHashSet());
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        Currency value)
    {
        var writer = context.Writer;
        writer.WriteStartDocument();
        writer.WriteName(ElementNames.Ticker);
        writer.WriteString(value.Ticker);
        writer.WriteName(ElementNames.DecimalPlaces);
        _decimalPlacesInfo.Serializer.Serialize(context, value.DecimalPlaces);
        if (value.Minters is not null)
        {
            writer.WriteName(ElementNames.Minters);
            _mintersInfo.Serializer.Serialize(context, value.Minters);
        }

        writer.WriteName(ElementNames.TotalSupplyTrackable);
        writer.WriteBoolean(value.TotalSupplyTrackable);
        if (value.MaximumSupply.HasValue)
        {
            writer.WriteName(ElementNames.MaximumSupplyMajor);
            _maximumSupplyMajorInfo.Serializer.Serialize(context, value.MaximumSupply.Value.MajorUnit);
            writer.WriteName(ElementNames.MaximumSupplyMinor);
            _maximumSupplyMinorInfo.Serializer.Serialize(context, value.MaximumSupply.Value.MinorUnit);
        }

        writer.WriteEndDocument();
    }
}
