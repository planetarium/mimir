using Nekoyume.Model.EnumType;

namespace Mimir.Models.Abstractions;

public interface IRuneSlot
{
    public int SlotIndex { get; }
    public RuneSlotType RuneSlotType { get; }
    public RuneType RuneType { get; }
    public bool IsLock { get; }
    public int? RuneSheetId { get; }
}
