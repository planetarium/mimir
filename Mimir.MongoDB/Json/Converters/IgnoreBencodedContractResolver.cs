using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        if (property.PropertyName == "Bencoded")
        {
            property.ShouldSerialize = instance => false;
        }

        return property;
    }
}
