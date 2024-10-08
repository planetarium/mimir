using System.Text.Json;
using System.Text.Json.Serialization;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Converters;

namespace Mimir.MongoDB.Json.Extensions;

public static class MimirBsonDocumentExtensions
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new BigIntegerJsonConverter(),
            new MaterialAndIntDictionaryJsonConverter(),
            new JsonStringEnumConverter(),
        },
    };

    public static string ToJson(this MimirBsonDocument document) =>
        JsonSerializer.Serialize(document, document.GetType(), JsonSerializerOptions);
}
