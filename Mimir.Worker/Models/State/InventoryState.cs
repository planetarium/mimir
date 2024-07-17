using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.Item;

namespace Mimir.Worker.Models;

public class InventoryState : IBencodable
{
    public Inventory Object;

    public InventoryState(Inventory inventory)
    {
        Object = inventory;
    }

    public IValue Bencoded => Object.Serialize();
}
