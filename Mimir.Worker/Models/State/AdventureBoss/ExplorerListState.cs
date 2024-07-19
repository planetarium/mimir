using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Models.State.AdventureBoss;

public record ExplorerListState(ExplorerList Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
