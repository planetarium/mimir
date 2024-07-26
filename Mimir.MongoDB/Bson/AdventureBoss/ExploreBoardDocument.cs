using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record ExploreBoardDocument(ExploreBoard Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded();
}
