using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record WorldBossDocument(
    int RaidId,
    Nekoyume.Model.State.WorldBossState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
