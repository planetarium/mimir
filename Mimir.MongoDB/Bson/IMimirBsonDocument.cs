using Libplanet.Crypto;
using Mimir.Worker.Json.Converters;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

public record IMimirBsonDocument(Address Address)
{
    public static readonly JsonSerializerSettings JsonSerializerSettings =
        new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = [new BigIntegerToStringConverter()],
            ContractResolver = new IgnoreBencodedContractResolver()
        };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
