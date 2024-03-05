using Newtonsoft.Json;
using NineChroniclesUtilBackend.Store.Util;

namespace NineChroniclesUtilBackend.Store.Models;

public class BaseData
{
    protected static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
    {
        Converters = { new BigIntegerToStringConverter(), new IStateJsonConverter() },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
