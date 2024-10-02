using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mimir.MongoDB.Json.Converters;

public class IgnoreBencodedPropertiesConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (T)JsonSerializer.Deserialize(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name.StartsWith("Bencoded")) continue;

            var propValue = prop.GetValue(value);

            if (propValue != null)
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, propValue, propValue.GetType(), options);
            }
            
        }

        writer.WriteEndObject();
    }
}
