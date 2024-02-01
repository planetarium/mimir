using Nekoyume.Model.State;

namespace NineChroniclesUtilBackend.Arena;

public record AvatarStatesForArena
{
    public AvatarState AvatarState { get; private set; }
    public ItemSlotState ItemSlotState { get;  private set; }
    public List<RuneState> RuneStates { get; private set; }

    public AvatarStatesForArena(AvatarState avatarState, ItemSlotState itemSlotState, List<RuneState> runeStates)
    {
        AvatarState = avatarState;
        ItemSlotState = itemSlotState;
        RuneStates = runeStates;
    }
}
