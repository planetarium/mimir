using Nekoyume.Model.Arena;
using Newtonsoft.Json;
using NineChroniclesUtilBackend.Store.Util;

namespace NineChroniclesUtilBackend.Store.Models;

public class ArenaData
{
    public ArenaScore Score { get; }
    public ArenaInformation Information { get; }

    public ArenaData(ArenaScore score, ArenaInformation information)
    {
        Score = score;
        Information = information;
    }

    public string ToJson()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new[] { new BigIntegerToStringConverter() },
            Formatting = Formatting.Indented
        };

        string jsonString = JsonConvert.SerializeObject(this, settings);
        return jsonString;
    }
}