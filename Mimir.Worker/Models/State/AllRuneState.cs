using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class AllRuneState(Address address, Nekoyume.Model.State.AllRuneState value) : State(address)
{
    public Nekoyume.Model.State.AllRuneState Value { get; set; } = value;
}
