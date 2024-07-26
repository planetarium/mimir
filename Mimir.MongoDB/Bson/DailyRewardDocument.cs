using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record DailyRewardDocument(long Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
