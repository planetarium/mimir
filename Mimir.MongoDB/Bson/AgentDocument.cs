using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AgentDocument(Address Address, Nekoyume.Model.State.AgentState Object)
    : MimirBsonDocument(Address) { }
