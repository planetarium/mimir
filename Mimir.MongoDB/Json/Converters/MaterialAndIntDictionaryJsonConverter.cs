using Lib9c.Models.Items;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Mimir.MongoDB.Json.Converters;

public class MaterialAndIntDictionaryJsonConverter : JsonConverter<Dictionary<Material, int>>
{
    // Deserialize (ReadJson equivalent)
    public override Dictionary<Material, int>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        var jsonNode = JsonNode.Parse(ref reader);
        if (jsonNode == null || jsonNode is not JsonObject jsonObject)
        {
            throw new JsonException("Invalid JSON format.");
        }

        var dictionary = new Dictionary<Material, int>();
        
        foreach (var property in jsonObject)
        {
            var material = JsonSerializer.Deserialize<Material>(property.Key);
            if (material is null)
            {
                throw new JsonException($"Failed to deserialize {nameof(Material)}. {property.Key}");
            }

            if (property.Value is JsonValue jsonValue && jsonValue.TryGetValue<int>(out var value))
            {
                dictionary.Add(material, value);
            }
            else
            {
                throw new JsonException($"Failed to convert value for {nameof(Material)} {property.Key}.");
            }
        }

        return dictionary;
    }

    // Serialize (WriteJson equivalent)
    public override void Write(Utf8JsonWriter writer, Dictionary<Material, int>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            var key = JsonSerializer.Serialize(kvp.Key, options);
            writer.WritePropertyName(key);
            writer.WriteNumberValue(kvp.Value);
        }
        writer.WriteEndObject();
    }
}
