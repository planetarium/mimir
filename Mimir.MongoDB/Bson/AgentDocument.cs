using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AgentDocument(Nekoyume.Model.State.AgentState Object) : IMimirBsonDocument
{
    public Address Address => Object.address;
    public IValue Bencoded => Object.SerializeList();
}
