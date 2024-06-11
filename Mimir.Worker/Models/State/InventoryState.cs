using Libplanet.Crypto;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class InventoryState : State
{
    public Inventory Object;

    public InventoryState(Address address, Inventory inventory)
        : base(address)
    {
        Object = inventory;
    }
}
