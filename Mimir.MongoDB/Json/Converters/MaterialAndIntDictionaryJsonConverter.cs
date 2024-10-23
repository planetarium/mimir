using Lib9c.Models.Items;
using Mimir.MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mimir.MongoDB.Json.Converters;

public class MaterialAndIntDictionaryJsonConverter : JsonConverter<Dictionary<Material, int>>
{
    public override Dictionary<Material, int>? ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<Material, int>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var dictionary = new Dictionary<Material, int>();
        var jObject = JObject.Load(reader);
        foreach (var property in jObject.Properties())
        {
            var material = JsonConvert.DeserializeObject<Material>(property.Name);
            if (material is null)
            {
                throw new JsonException($"Failed to deserialize {nameof(Material)}. {property.Name}");
            }

            var value = property.Value.ToObject<int>();
            dictionary.Add(material, value);
        }

        return dictionary;
    }

    public override void WriteJson(
        JsonWriter writer,
        Dictionary<Material, int>? value,
        JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            var key = JsonConvert.SerializeObject(kvp.Key, MimirBsonDocumentExtensions.JsonSerializerSettings);
            writer.WritePropertyName(key);
            writer.WriteValue(kvp.Value);
        }

        writer.WriteEndObject();
    }
}
