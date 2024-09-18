using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class WorldInformationStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        return new WorldInformationDocument(
            context.Address,
            new WorldInformationState(context.RawState)
        );
    }
}
