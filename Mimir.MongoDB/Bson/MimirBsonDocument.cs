using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

public record MimirBsonDocument([property: BsonId] Address Address, DocumentMetadata metadata);
