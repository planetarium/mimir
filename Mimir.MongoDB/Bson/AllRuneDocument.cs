using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AllRuneDocument(
    Address Address,
    Nekoyume.Model.State.AllRuneState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
