using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record AgentDocument(Nekoyume.Model.State.AgentState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.SerializeList();
}
