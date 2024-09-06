using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record WorldBossStateDocument(
    Address Address,
    int RaidId,
    WorldBossState Object
) : MimirBsonDocument(Address) { }
