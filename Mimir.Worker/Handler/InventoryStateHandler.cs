using Bencodex;
using Bencodex.Types;
using Mimir.MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Worker.Handler;

public class InventoryStateHandler : IStateHandler
{
    public IBencodable ConvertToState(StateDiffContext context)
    {
        if (context.RawState is List list)
        {
            return new InventoryState(new Inventory(list));
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected List."
            );
        }
    }
}
