using Bencodex.Types;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public class ActionPointStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.RawState));

    public async Task StoreStateData(MongoDbService store, StateData stateData) =>
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

    private static ActionPointState ConvertToState(IValue state)
    {
        if (state is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}."
            );
        }

        return new ActionPointState(value);
    }
}
