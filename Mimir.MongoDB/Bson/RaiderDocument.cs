using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record RaiderDocument(
    Address Address,
    Nekoyume.Model.State.RaiderState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
