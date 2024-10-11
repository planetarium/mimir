using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateDiffHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        return new AvatarDocument(context.Address, new AvatarState(context.RawState));
    }
}
