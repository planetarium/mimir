using Bencodex;
using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public class WorldBossState(
    int raidId,
    Nekoyume.Model.State.WorldBossState worldBossState
) : IBencodable
{
    public int raidId { get; set; } = raidId;

    public Nekoyume.Model.State.WorldBossState Object { get; set; } = worldBossState;

    public IValue Bencoded => Object.Serialize();
}
