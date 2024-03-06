using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nekoyume.Model.State;
using Libplanet.Common;
using Bencodex;
using Bencodex.Types;

namespace NineChroniclesUtilBackend.Store.Util;

public class IStateJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(IState).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, JsonSerializer.CreateDefault(new JsonSerializerSettings { 
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>() { new BigIntegerToStringConverter() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        }));
        
        IValue? ivalue = value switch
        {
            AvatarState avatarState => avatarState.SerializeList(),
            IState state => state.Serialize(),
            _ => null
        };

        if (ivalue != null)
        {
            string rawValue = ByteUtil.Hex(new Codec().Encode(ivalue));
            jo.Add("Raw", rawValue);
        }
        
        jo.WriteTo(writer);
    }
}
