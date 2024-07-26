using Mimir.Worker.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Mimir.MongoDB.Bson;

public record BaseData
{
    protected static JsonSerializerSettings JsonSerializerSettings => new()
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
