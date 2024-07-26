using Libplanet.Crypto;
using Mimir.Worker.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mimir.MongoDB.Bson;

public record MongoDbCollectionDocument(Address Address, IMimirBsonDocument State)
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = [new BigIntegerToStringConverter()],
        ContractResolver = new DefaultContractResolver { IgnoreSerializableInterface = true }
    };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
