using Bencodex.Types;
using Nekoyume.Model.Stake;

namespace Mimir.MongoDB.Bson;

public record StakeDocument(StakeStateV2 Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
