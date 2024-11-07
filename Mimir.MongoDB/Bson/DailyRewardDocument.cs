using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record DailyRewardDocument(long StoredBlockIndex, Address Address, long Object)
    : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
