using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record PetDocument(
    Address Address,
    Nekoyume.Model.State.PetState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
