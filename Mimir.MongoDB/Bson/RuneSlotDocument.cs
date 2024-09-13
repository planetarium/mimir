using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record RuneSlotDocument(Address Address, RuneSlotState Object) : MimirBsonDocument(Address);
