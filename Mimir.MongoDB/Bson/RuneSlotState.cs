using Bencodex;
using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public class RuneSlotState(Lib9c.Models.States.RuneSlotState runeSlotState) : IBencodable
{
    public Lib9c.Models.States.RuneSlotState Object { get; } = runeSlotState;
    public IValue Bencoded => Object.Bencoded;
}
