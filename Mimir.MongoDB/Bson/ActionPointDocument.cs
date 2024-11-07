using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ActionPointDocument(long StoredBlockIndex, Address Address, int Object)
    : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
