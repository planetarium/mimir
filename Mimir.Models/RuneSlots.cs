using Libplanet.Crypto;
using Mimir.Models.Abstractions;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Models;

public class RuneSlots(
    Address address,
    BattleType battleType,
    IEnumerable<IRuneSlot> slots)
    : StateModel(address), IRuneSlots
{
    public BattleType BattleType { get; } = battleType;
    public IEnumerable<IRuneSlot> Slots { get; } = slots;

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
