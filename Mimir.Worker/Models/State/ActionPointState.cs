using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ActionPointState(Address address, int value) : State(address)
{
    public int Object { get; set; } = value;

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
