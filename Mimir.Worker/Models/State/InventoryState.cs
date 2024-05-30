using Libplanet.Crypto;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class InventoryState : State
{
    public Inventory Inventory;

    public InventoryState(Address address, Inventory inventory)
        : base(address)
    {
        Inventory = inventory;
    }
}
