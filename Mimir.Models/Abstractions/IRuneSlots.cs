using Nekoyume.Model.EnumType;

namespace Mimir.Models.Abstractions;

public interface IRuneSlots : IStateModel
{
    BattleType BattleType { get; }
    IEnumerable<IRuneSlot> Slots { get; }
}
