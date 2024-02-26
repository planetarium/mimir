using Newtonsoft.Json;
using NineChroniclesUtilBackend.Store.Util;

namespace NineChroniclesUtilBackend.Store.Models;

public class BaseData
{
    protected static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
    {
        Converters = new[] { new BigIntegerToStringConverter() },
        Formatting = Formatting.Indented,
        // ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, JsonSerializerSettings);
    }
}
