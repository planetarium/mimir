using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Models.State.AdventureBoss;

public record ExplorerState(Explorer Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
