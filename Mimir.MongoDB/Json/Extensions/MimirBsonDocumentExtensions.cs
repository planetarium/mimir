using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Converters;
using Newtonsoft.Json;

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
        ],
        ContractResolver = new IgnoreBencodedContractResolver(),
    };

    public static string ToJson(this MimirBsonDocument document) =>
        JsonConvert.SerializeObject(document, JsonSerializerSettings);
}
