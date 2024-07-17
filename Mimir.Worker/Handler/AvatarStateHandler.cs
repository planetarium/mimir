using Bencodex.Types;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

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
            return new AvatarState(new Nekoyume.Model.State.AvatarState(dictionary));
        }
        else if (state is List alist)
        {
            return new AvatarState(new Nekoyume.Model.State.AvatarState(alist));
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected Dictionary or List."
            );
        }
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsync(stateData);
    }
}
