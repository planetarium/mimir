using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using Nekoyume.Model.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Mimir.Worker.Util;

public class StateJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(IState).IsAssignableFrom(objectType);
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        long initialMemory = GC.GetTotalMemory(false);

        JObject jo = JObject.FromObject(
            value,
            JsonSerializer.CreateDefault(
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = new List<JsonConverter> { new BigIntegerToStringConverter() },
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        IgnoreSerializableInterface = true
                    }
                }
            )
        );

        IValue? ivalue = value switch
        {
            AvatarState avatarState => avatarState.SerializeList(),
            AgentState agentState => agentState.SerializeList(),
            State state => state.Serialize(),
            _ => null
        };

        if (ivalue != null)
        {
            string rawValue = ByteUtil.Hex(new Codec().Encode(ivalue));
            jo.Add("Raw", rawValue);
        }

        jo.WriteTo(writer);

        long finalMemory = GC.GetTotalMemory(false);
        long memoryUsed = finalMemory - initialMemory;

        if (memoryUsed > 100_000_000)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
