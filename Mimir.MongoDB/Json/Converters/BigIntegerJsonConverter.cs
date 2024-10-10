using System.Numerics;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Json.Converters;

public class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
    public override BigInteger ReadJson(
        JsonReader reader,
        Type objectType,
        BigInteger existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            throw new JsonSerializationException("Cannot convert null value to BigInteger.");
        }

        if (reader.TokenType == JsonToken.String)
        {
            var value = (string)reader.Value!;
            return BigInteger.Parse(value);
        }

        if (reader.TokenType == JsonToken.Integer)
        {
            var value = (long)reader.Value!;
            return new BigInteger(value);
        }

        throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}.");
    }

    public override void WriteJson(
        JsonWriter writer,
        BigInteger value,
        JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
