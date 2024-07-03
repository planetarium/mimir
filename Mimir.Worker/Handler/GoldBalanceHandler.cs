using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.State;
using GoldBalanceState = Mimir.Worker.Models.GoldBalanceState;

namespace Mimir.Worker.Handler;

public class GoldBalanceHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var goldBalance = ConvertToState(context.Address, context.RawState);
        return new StateData(context.Address, goldBalance);
    }

    private GoldBalanceState ConvertToState(Address address, IValue state)
    {
        if (state is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}."
            );
        }

        return new GoldBalanceState(address, value.ToBigInteger());
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsync(stateData);
    }
}
