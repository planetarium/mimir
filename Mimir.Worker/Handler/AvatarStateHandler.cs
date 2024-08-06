using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AvatarStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToState(StateDiffContext context)
    {
        return new AvatarDocument(context.Address, new Lib9c.Models.States.AvatarState(context.RawState));
    }
}
