using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldBossKillRewardRecordState(
    Address address,
    Address avatarAddress,
    WorldBossKillRewardRecord worldBossKillRewardRecord
) : State(address)
{
    public Address AvatarAddress { get; set; } = avatarAddress;
    public WorldBossKillRewardRecord Object { get; set; } = worldBossKillRewardRecord;

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
