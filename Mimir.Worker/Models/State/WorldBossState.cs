using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldBossState(
    Address address,
    int raidId,
    Nekoyume.Model.State.WorldBossState worldBossState
) : State(address)
{
    public int raidId { get; set; } = raidId;

    public Nekoyume.Model.State.WorldBossState Object { get; set; } = worldBossState;
}
