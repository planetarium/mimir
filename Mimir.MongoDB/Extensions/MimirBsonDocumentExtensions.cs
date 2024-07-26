using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mimir.MongoDB.Extensions;

public static class MimirBsonDocumentExtensions
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = [new BigIntegerToStringConverter()],
        ContractResolver = new DefaultContractResolver { IgnoreSerializableInterface = true }
    };

    public static string ToJson(this IMimirBsonDocument document)
    {
        return JsonConvert.SerializeObject(document, JsonSerializerSettings);
    }
}
