using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record SeasonInfoState(SeasonInfo Object, IValue Bencoded) : IBencodable;
