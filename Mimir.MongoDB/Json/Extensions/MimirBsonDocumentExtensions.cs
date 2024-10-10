using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mimir.MongoDB.Json.Extensions;

public static class MimirBsonDocumentExtensions
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters =
        [
            new BigIntegerJsonConverter(),
            new MaterialAndIntDictionaryJsonConverter(),
            new StringEnumConverter(),
        ],
        ContractResolver = new IgnoreBencodedContractResolver(),
    };

    public static string ToJson(this MimirBsonDocument document) =>
        JsonConvert.SerializeObject(document, JsonSerializerSettings);
}
