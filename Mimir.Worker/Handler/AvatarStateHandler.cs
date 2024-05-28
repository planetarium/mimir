using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(Address address, IValue rawState)
    {
        var avatarState = ConvertToState(rawState);
        return new StateData(address, avatarState);
    }

    public StateData ConvertToStateData(Address address, string rawState)
    {
        Codec Codec = new();
        var state = Codec.Decode(Convert.FromHexString(rawState));
        var avatarState = ConvertToState(state);

        return new StateData(address, avatarState);
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
}
