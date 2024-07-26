using Bencodex.Types;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record ExplorerListDocument(ExplorerList Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
