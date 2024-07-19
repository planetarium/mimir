using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Models.State.AdventureBoss;

public record BountyBoardState(BountyBoard Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
