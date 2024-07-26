using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record BountyBoardState(BountyBoard Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
