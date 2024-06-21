using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldBossState(Address address, Nekoyume.Model.State.WorldBossState worldBossState)
    : State(address)
{
    public Nekoyume.Model.State.WorldBossState Object { get; set; } = worldBossState;
}
