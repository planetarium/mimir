using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public class WorldBossKillRewardRecordState(
    Address avatarAddress,
    WorldBossKillRewardRecord worldBossKillRewardRecord
) : IBencodable
{
    public Address AvatarAddress { get; set; } = avatarAddress;

    public WorldBossKillRewardRecord Object { get; set; } = worldBossKillRewardRecord;

    public IValue Bencoded => Object.Serialize();
}
