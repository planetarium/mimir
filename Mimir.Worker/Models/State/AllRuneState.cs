using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class AllRuneState(Address address, Nekoyume.Model.State.AllRuneState allRuneState) : State(address)
{
    public Nekoyume.Model.State.AllRuneState Object { get; set; } = allRuneState;
}
