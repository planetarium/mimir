using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldBossStateDocument(
    Address Address,
    int RaidId,
    WorldBossState Object)
    : MimirBsonDocument(Address);
