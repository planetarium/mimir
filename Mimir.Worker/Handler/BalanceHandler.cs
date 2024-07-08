using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public record BalanceHandler(Currency Currency) : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var goldBalance = ConvertToState(context.Address, context.RawState);
        return new StateData(context.Address, goldBalance);
    }

    private BalanceState ConvertToState(Address address, IValue state)
    {
        if (state is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}."
            );
        }

        return new BalanceState(address, FungibleAssetValue.FromRawValue(Currency, value));
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsync(stateData);
    }
}
