using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Runes;

namespace Mimir.Worker.Models;

public class RuneSlotState(RuneSlots runeSlots) : IBencodable
{
    public RuneSlots Object { get; } = runeSlots;
    // Can't Bencodable
    public IValue Bencoded => Object.Address.Bencoded;
}
