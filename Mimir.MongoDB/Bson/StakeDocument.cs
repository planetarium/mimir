using System.Numerics;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record StakeDocument(Address Address, Address AgentAddress, StakeState? Object, BigInteger Amount) : MimirBsonDocument(Address);
