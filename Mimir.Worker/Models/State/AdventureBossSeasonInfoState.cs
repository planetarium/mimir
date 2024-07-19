using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Models;

public record AdventureBossSeasonInfoState(SeasonInfo Object) : IBencodable
{
    public IValue Bencoded => Object.Bencoded;
}
