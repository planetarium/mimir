using Mimir.Worker.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mimir.Worker.Models;

public record BaseData
{
    protected static JsonSerializerSettings JsonSerializerSettings =>
        new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter> { new BigIntegerToStringConverter() },
            ContractResolver = new DefaultContractResolver { IgnoreSerializableInterface = true }
        };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
