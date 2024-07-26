using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record WorldBossDocument(
    Address Address,
    Nekoyume.Model.State.WorldBossState Object,
    int RaidId)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
