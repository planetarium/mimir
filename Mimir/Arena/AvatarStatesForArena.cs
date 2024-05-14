using Nekoyume.Model.State;

namespace Mimir.Arena;

public record AvatarStatesForArena
{
    public AvatarState AvatarState { get; private set; }
    public ItemSlotState ItemSlotState { get;  private set; }
    public AllRuneState RuneStates { get; private set; }
    public RuneSlotState RuneSlotState { get; private set; }

    public AvatarStatesForArena(AvatarState avatarState, ItemSlotState itemSlotState, AllRuneState runeStates, RuneSlotState runeSlotState)
    {
        AvatarState = avatarState;
        ItemSlotState = itemSlotState;
        RuneStates = runeStates;
        RuneSlotState = runeSlotState;
    }
}
