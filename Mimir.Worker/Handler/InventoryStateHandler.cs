using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.Item;

namespace Mimir.Worker.Handler;

public class InventoryStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var inventoryState = ConvertToState(context.RawState);
        return new StateData(context.Address, new InventoryState(context.Address, inventoryState));
    }

    private Inventory ConvertToState(IValue state)
    {
        if (state is List list)
        {
            return new Inventory(list);
        }
        else
        {
            throw new InvalidCastException($"{nameof(state)} Invalid state type. Expected List.");
        }
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);
    }
}
