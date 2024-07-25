using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Runes;

namespace Mimir.Worker.Models;

public class RuneSlotState(Lib9c.Models.States.RuneSlotState runeSlotState) : IBencodable
{
    public Lib9c.Models.States.RuneSlotState Object { get; } = runeSlotState;
    public IValue Bencoded => Object.Bencoded;
}
