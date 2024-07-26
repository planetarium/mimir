using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record AllRuneDocument(Nekoyume.Model.State.AllRuneState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
