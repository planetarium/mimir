using Bencodex.Types;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var avatarState = ConvertToState(context.RawState);
        return new StateData(context.Address, avatarState);
    }

    private AvatarState ConvertToState(IValue state)
    {
        if (state is Dictionary dictionary)
        {
            return new AvatarState(dictionary);
        }
        else if (state is List alist)
        {
            return new AvatarState(alist);
        }
        else
        {
            throw new ArgumentException(
                "Invalid state type. Expected Dictionary or List.",
                nameof(state)
            );
        }
    }

    public async Task StoreStateData(DiffMongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsync(stateData);
    }
}
