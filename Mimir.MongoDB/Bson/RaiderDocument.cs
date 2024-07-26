using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record RaiderDocument(Nekoyume.Model.State.RaiderState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
