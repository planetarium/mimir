using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Stake;

namespace Mimir.MongoDB.Bson;

public record StakeDocument(
    Address Address,
    StakeStateV2 Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
