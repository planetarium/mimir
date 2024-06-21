using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldBossKillRewardRecordState(
    Address address,
    WorldBossKillRewardRecord worldBossKillRewardRecord
) : State(address)
{
    public WorldBossKillRewardRecord Object { get; set; } = worldBossKillRewardRecord;
}
