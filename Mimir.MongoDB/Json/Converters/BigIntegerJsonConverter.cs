using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mimir.MongoDB.Json.Converters;

public class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException("Cannot convert null value to BigInteger.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return BigInteger.Parse(value!);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long value))
            {
                return new BigInteger(value);
            }
            
            throw new JsonException("Invalid number format for BigInteger.");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
