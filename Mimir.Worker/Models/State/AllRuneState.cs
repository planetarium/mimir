using Bencodex;
using Bencodex.Types;

namespace Mimir.Worker.Models;

public class AllRuneState(Nekoyume.Model.State.AllRuneState allRuneState)
    : IBencodable
{
    public Nekoyume.Model.State.AllRuneState Object { get; set; } = allRuneState;

    public IValue Bencoded => Object.Serialize();
}
