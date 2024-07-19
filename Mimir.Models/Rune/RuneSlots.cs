using Libplanet.Crypto;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Models.Rune;

public record RuneSlots(
    Address Address,
    BattleType BattleType,
    IEnumerable<RuneSlot> Slots)
    : StateModel(Address)
{
    public RuneSlots(
        Address address,
        RuneSlotState runeSlotState)
        : this(
            address,
            runeSlotState.BattleType,
            runeSlotState.GetRuneSlot().Select(runeSlot => new RuneSlot(runeSlot)))
    {
    }
}
