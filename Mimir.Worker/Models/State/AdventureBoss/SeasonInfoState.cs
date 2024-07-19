using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Models.State.AdventureBoss;

public record SeasonInfoState(SeasonInfo Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
