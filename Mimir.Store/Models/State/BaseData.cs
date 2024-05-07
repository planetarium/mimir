using Newtonsoft.Json;
using Mimir.Store.Util;

namespace Mimir.Store.Models;

public class BaseData
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
