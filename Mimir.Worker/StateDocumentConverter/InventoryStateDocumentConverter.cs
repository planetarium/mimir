using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Lib9c.Models.Items;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class InventoryStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        return new InventoryDocument(
            context.BlockIndex,
            context.Address,
            new Inventory(context.RawState)
        );
    }
}
