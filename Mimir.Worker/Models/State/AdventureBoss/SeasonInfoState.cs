using Bencodex;
using Bencodex.Types;
using Mimir.Models.AdventureBoss;

namespace Mimir.Worker.Models.State.AdventureBoss;

public record SeasonInfoState(SeasonInfo Object, IValue Bencoded) : IBencodable;
