using Mimir.Models.Abstractions;
using Nekoyume.Model.EnumType;

namespace Mimir.Models;

public class RuneSlot(
    int slotIndex,
    RuneSlotType runeSlotType,
    RuneType runeType,
    bool isLock,
    int? runeSheetId)
    : IRuneSlot
{
    public int SlotIndex { get; } = slotIndex;
    public RuneSlotType RuneSlotType { get; } = runeSlotType;
    public RuneType RuneType { get; } = runeType;
    public bool IsLock { get; } = isLock;
    public int? RuneSheetId { get; } = runeSheetId;

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
