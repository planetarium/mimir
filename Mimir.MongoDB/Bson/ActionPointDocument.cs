using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record ActionPointDocument(Address Address, int Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
