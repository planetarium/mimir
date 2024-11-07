using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldBossKillRewardRecordDocument(
    long StoredBlockIndex,
    Address Address,
    Address AvatarAddress,
    WorldBossKillRewardRecord Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
