using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public class ActionPointStateHandler : IStateHandler<StateData>
{
    private readonly Codec _codec = new();

    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.Address, context.RawState));

    private static ActionPointState ConvertToState(Address address, IValue state)
    {
        if (state is not Integer value)
        {
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}.",
                nameof(state)
            );
        }

        return new ActionPointState(address, value);
    }

    public async Task StoreStateData(DiffMongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);
    }
}
