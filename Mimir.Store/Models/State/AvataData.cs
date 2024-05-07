using Nekoyume.Model.State;

namespace Mimir.Store.Models;

public class AvatarData : BaseData
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