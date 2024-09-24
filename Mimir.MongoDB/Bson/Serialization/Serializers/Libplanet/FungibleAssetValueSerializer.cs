using System.Numerics;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
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

    private static class Flags
    {
        public const long Currency = 1L << 0;
        public const long RawValue = 1L << 1;
    }

    public static readonly FungibleAssetValueSerializer Instance = new();

    private readonly SerializerHelper _helper = new(
        new SerializerHelper.Member(ElementNames.Currency, Flags.Currency),
        new SerializerHelper.Member(ElementNames.RawValue, Flags.RawValue));

    private readonly BsonSerializationInfo _currencyInfo = new(
        ElementNames.Currency,
        CurrencySerializer.Instance,
        typeof(Currency));

    private readonly BsonSerializationInfo _rawValueInfo = new(
        ElementNames.RawValue,
        BigIntegerSerializer.Instance,
        typeof(BigInteger));

    public override FungibleAssetValue Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        Currency currency = default;
        BigInteger rawValue = default;
        _helper.DeserializeMembers(context, (_, flag) =>
        {
            switch (flag)
            {
                case Flags.Currency:
                    currency = (Currency)_currencyInfo.Serializer.Deserialize(context);
                    break;
                case Flags.RawValue:
                    rawValue = (BigInteger)_rawValueInfo.Serializer.Deserialize(context);
                    break;
            }
        });
        return FungibleAssetValue.FromRawValue(currency, rawValue);
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        FungibleAssetValue value)
    {
        var writer = context.Writer;
        writer.WriteStartDocument();
        writer.WriteName(ElementNames.Currency);
        _currencyInfo.Serializer.Serialize(context, value.Currency);
        writer.WriteName(ElementNames.RawValue);
        _rawValueInfo.Serializer.Serialize(context, value.RawValue);
        writer.WriteEndDocument();
    }
}
