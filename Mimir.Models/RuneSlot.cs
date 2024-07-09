using Mimir.Models.Abstractions;
using Nekoyume.Model.EnumType;

namespace Mimir.Models;

public record RuneSlot(
    int SlotIndex,
    RuneSlotType RuneSlotType,
    RuneType RuneType,
    bool IsLock,
    int? RuneSheetId)
    : IRuneSlot
{
    public RuneSlot(Nekoyume.Model.Rune.RuneSlot runeSlot)
        : this(
            runeSlot.Index,
            runeSlot.RuneSlotType,
            runeSlot.RuneType,
            runeSlot.IsLock,
            runeSlot.RuneId)
    {
    }
}
