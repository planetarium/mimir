using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume.Model.Item;

namespace Mimir.Worker.Handler;

public class InventoryStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(Address address, IValue rawState)
    {
        var inventoryState = ConvertToState(rawState);
        return new StateData(address, new InventoryState(address, inventoryState));
    }

    public StateData ConvertToStateData(Address address, string rawState)
    {
        Codec Codec = new();
        var state = Codec.Decode(Convert.FromHexString(rawState));
        var inventoryState = ConvertToState(state);

        return new StateData(address, new InventoryState(address, inventoryState));
    }

    private Inventory ConvertToState(IValue state)
    {
        if (state is List list)
        {
            return new Inventory(list);
        }
        else
        {
            throw new ArgumentException("Invalid state type. Expected List.", nameof(state));
        }
    }
}
