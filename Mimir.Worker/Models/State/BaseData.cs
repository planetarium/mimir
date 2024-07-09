using Newtonsoft.Json;
using Mimir.Worker.Util;

namespace Mimir.Worker.Models;

public record BaseData
{
    protected static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
    {
        Converters = { new StateJsonConverter() },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
