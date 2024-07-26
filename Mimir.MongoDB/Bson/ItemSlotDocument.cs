using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record ItemSlotDocument(Nekoyume.Model.State.ItemSlotState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
