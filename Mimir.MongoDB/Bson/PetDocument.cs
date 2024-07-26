using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record PetDocument(Nekoyume.Model.State.PetState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
