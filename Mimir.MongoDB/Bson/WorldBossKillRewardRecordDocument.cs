using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record WorldBossKillRewardRecordDocument(
    Address AvatarAddress,
    WorldBossKillRewardRecord Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
