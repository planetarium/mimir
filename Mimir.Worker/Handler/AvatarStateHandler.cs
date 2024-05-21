using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler<AvatarState>
{
    public AvatarState ConvertToState(string rawState)
    {
        Codec Codec = new();
        var state = Codec.Decode(Convert.FromHexString(rawState));
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
