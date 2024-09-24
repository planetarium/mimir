using System.Collections.Immutable;
using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
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
        public const string MaximumSupply = "MaximumSupply";
    }

    private static class Flags
    {
        public const long Ticker = 1L << 0;
        public const long DecimalPlaces = 1L << 1;
        public const long Minters = 1L << 2;
        public const long TotalSupplyTrackable = 1L << 3;
        public const long MaximumSupply = 1L << 4;
    }

    public static readonly CurrencySerializer Instance = new();

    private readonly SerializerHelper _helper = new(
        new SerializerHelper.Member(ElementNames.Ticker, Flags.Ticker),
        new SerializerHelper.Member(ElementNames.DecimalPlaces, Flags.DecimalPlaces),
        new SerializerHelper.Member(ElementNames.Minters, Flags.Minters),
        new SerializerHelper.Member(ElementNames.TotalSupplyTrackable, Flags.TotalSupplyTrackable),
        new SerializerHelper.Member(ElementNames.MaximumSupply, Flags.MaximumSupply));

    private readonly BsonSerializationInfo _decimalPlacesInfo = new(
        ElementNames.DecimalPlaces,
        new ByteSerializer(),
        typeof(byte));

    private readonly BsonSerializationInfo _mintersInfo = new(
        ElementNames.Minters,
        new ArraySerializer<AddressSerializer>(),
        typeof(IImmutableSet<Address>));

    private readonly BsonSerializationInfo _maximumSupplyInfo = new(
        ElementNames.MaximumSupply,
        new NullableSerializer<(BigInteger, BigInteger)>(
            new ValueTupleSerializer<BigInteger, BigInteger>(
                BigIntegerSerializer.Instance,
                BigIntegerSerializer.Instance)),
        typeof((BigInteger, BigInteger)?));

    public override Currency Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var ticker = string.Empty;
        byte decimalPlaces = default;
        Address[]? minters = null;
        bool totalSupplyTrackable = default;
        (BigInteger majorUnit, BigInteger minorUnit)? maximumSupply = null;
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
                case Flags.MaximumSupply:
                    maximumSupply =
                        ((BigInteger majorUnit, BigInteger minorUnit)?)_maximumSupplyInfo.Serializer
                            .Deserialize(context);
                    break;
            }
        });

        if (totalSupplyTrackable)
        {
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
        writer.WriteName(ElementNames.Minters);
        _mintersInfo.Serializer.Serialize(context, value.Minters);
        writer.WriteName(ElementNames.TotalSupplyTrackable);
        writer.WriteBoolean(value.TotalSupplyTrackable);
        writer.WriteName(ElementNames.MaximumSupply);
        _maximumSupplyInfo.Serializer.Serialize(context, value.MaximumSupply);
        writer.WriteEndDocument();
    }
}
