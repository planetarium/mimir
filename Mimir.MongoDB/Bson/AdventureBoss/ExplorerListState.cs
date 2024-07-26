using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record ExplorerListState(ExplorerList Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
