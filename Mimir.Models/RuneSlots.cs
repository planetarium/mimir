using Libplanet.Crypto;
using Mimir.Models.Abstractions;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Models;

public record RuneSlots(
    Address Address,
    BattleType BattleType,
    IEnumerable<IRuneSlot> Slots)
    : StateModel(Address), IRuneSlots
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
