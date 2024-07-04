using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class RaiderState(Address address, Nekoyume.Model.State.RaiderState raiderState)
    : State(address)
{
    public Nekoyume.Model.State.RaiderState Object { get; set; } = raiderState;

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
