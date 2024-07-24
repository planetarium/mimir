using Bencodex;
using Bencodex.Types;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler
{
    public IBencodable ConvertToState(StateDiffContext context)
    {
        if (context.RawState is Dictionary dictionary)
        {
            return new AvatarState(new Nekoyume.Model.State.AvatarState(dictionary));
        }
        else if (context.RawState is List alist)
        {
            return new AvatarState(new Nekoyume.Model.State.AvatarState(alist));
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected Dictionary or List."
            );
        }
    }
}
