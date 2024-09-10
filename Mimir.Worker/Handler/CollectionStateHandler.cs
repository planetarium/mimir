using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class CollectionStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        return new CollectionDocument(context.Address, new CollectionState(context.RawState));
    }
}
