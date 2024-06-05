using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class CollectionState(Address address, Nekoyume.Model.State.CollectionState collectionState)
    : State(address)
{
    public Nekoyume.Model.State.CollectionState Object { get; set; } = collectionState;
}
