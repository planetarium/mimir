using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record WorldBossKillRewardRecordDocument(
    Address Address,
    WorldBossKillRewardRecord Object,
    Address AvatarAddress)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
