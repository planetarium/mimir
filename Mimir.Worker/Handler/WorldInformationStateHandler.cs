using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class WorldInformationStateHandler : IStateDiffHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        return new WorldInformationDocument(
            context.Address,
            new WorldInformationState(context.RawState)
        );
    }
}
