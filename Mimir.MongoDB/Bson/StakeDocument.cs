using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record StakeDocument(Address Address, Address agentAddress, StakeState Object) : MimirBsonDocument(Address);
