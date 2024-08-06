using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mimir.MongoDB.Json.Converters;

public class IgnoreBencodedContractResolver : DefaultContractResolver
{
    public IgnoreBencodedContractResolver()
    {
        IgnoreSerializableInterface = true;
    }

    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyName is not null && property.PropertyName.StartsWith("Bencoded"))
        {
            property.ShouldSerialize = instance => false;
        }

        return property;
    }
}
