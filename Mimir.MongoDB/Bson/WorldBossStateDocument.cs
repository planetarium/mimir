using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldBossStateDocument(
    long StoredBlockIndex,
    Address Address,
    int RaidId,
    WorldBossState Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
