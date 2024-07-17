using Bencodex;
using Bencodex.Types;

namespace Mimir.Worker.Models;

public class CollectionState(Nekoyume.Model.State.CollectionState collectionState) : IBencodable
{
    public Nekoyume.Model.State.CollectionState Object { get; set; } = collectionState;

    public IValue Bencoded => Object.Bencoded;
}
