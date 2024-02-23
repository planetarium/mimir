using Nekoyume.Model.State;
using Newtonsoft.Json;
using NineChroniclesUtilBackend.Store.Util;


namespace NineChroniclesUtilBackend.Store.Models;

public class AvatarData
{
    public AvatarState Avatar { get; }
    public ItemSlotState ItemSlot { get; }
    public List<RuneState> RuneSlot { get; }

    public AvatarData(AvatarState avatar, ItemSlotState itemSlot, List<RuneState> runeSlot)
    {
        Avatar = avatar;
        ItemSlot = itemSlot;
        RuneSlot = runeSlot;
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