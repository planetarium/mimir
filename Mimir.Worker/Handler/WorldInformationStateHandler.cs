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
            new WorldInformationState(context.Address, worldInformation)
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
            throw new ArgumentException("Invalid state type. Expected Dictionary.", nameof(state));
        }
    }

    public async Task StoreStateData(DiffMongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData);
    }
}
