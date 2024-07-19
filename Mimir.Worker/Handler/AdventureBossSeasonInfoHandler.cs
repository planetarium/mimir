using Bencodex.Types;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Handler;

public class AdventureBossSeasonInfoHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.RawState));

    public async Task StoreStateData(MongoDbService store, StateData stateData) =>
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

    private static AdventureBossSeasonInfoState ConvertToState(IValue state)
    {
        if (state is not List list)
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}."
            );
        }

        var seasonInfo = new SeasonInfo(list);
        return new AdventureBossSeasonInfoState(seasonInfo);
    }
}
