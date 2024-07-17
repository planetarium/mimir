using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model;

namespace Mimir.Worker.Handler;

public class WorldInformationStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var worldInformation = ConvertToState(context.RawState);
        return new StateData(
            context.Address,
            new WorldInformationState(worldInformation)
        );
    }

    private WorldInformation ConvertToState(IValue state)
    {
        if (state is Dictionary dict)
        {
            return new WorldInformation(dict);
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected Dictionary."
            );
        }
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);
    }
}
