using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Runes;

namespace Mimir.Worker.Models;

public class RuneSlotState(Lib9c.Models.Runes.RuneSlotState runeSlotState) : IBencodable
{
    public Lib9c.Models.Runes.RuneSlotState Object { get; } = runeSlotState;
    public IValue Bencoded => Object.Bencoded;
}
