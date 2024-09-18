using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldInformationDocument(Address Address, WorldInformationState Object)
    : MimirBsonDocument(Address);
