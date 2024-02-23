using Nekoyume.Model.State;

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
}