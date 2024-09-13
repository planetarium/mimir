using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ActionPointDocument(Address Address, int Object) : MimirBsonDocument(Address);
