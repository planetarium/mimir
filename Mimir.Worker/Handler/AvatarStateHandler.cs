using Bencodex;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler
{
    public IBencodable ConvertToState(StateDiffContext context)
    {
        return new AvatarState(new Lib9c.Models.States.AvatarState(context.RawState));
    }
}
