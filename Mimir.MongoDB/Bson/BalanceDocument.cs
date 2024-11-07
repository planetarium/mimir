using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record BalanceDocument(long StoredBlockIndex, Address Address, string Object)
    : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
