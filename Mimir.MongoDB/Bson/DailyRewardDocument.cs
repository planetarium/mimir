using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record DailyRewardDocument(
    Address Address,
    long Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
