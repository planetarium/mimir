using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public class DailyRewardStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.RawState));

    private static DailyRewardState ConvertToState(IValue state)
    {
        if (state is not Integer value)
        {
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}.",
                nameof(state)
            );
        }

        return new DailyRewardState(value);
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);
    }
}
