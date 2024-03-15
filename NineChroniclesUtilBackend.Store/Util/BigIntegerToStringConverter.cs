using System.Numerics;
using Newtonsoft.Json;

namespace NineChroniclesUtilBackend.Store.Util;

public class BigIntegerToStringConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(BigInteger);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
