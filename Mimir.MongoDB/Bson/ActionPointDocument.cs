using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record ActionPointDocument(int Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
