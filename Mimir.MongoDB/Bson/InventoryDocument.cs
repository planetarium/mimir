using Bencodex.Types;
using Nekoyume.Model.Item;

namespace Mimir.MongoDB.Bson;

public record InventoryDocument(Inventory Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
