using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record BountyBoardDocument(BountyBoard Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
