using Bencodex.Types;
using Mimir.MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Worker.Handler;

public class InventoryStateHandler : IStateHandler
{
    public IMimirBsonDocument ConvertToState(StateDiffContext context)
    {
        if (context.RawState is List list)
        {
            return new InventoryDocument(context.Address, new Inventory(list));
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected List."
            );
        }
    }
}
