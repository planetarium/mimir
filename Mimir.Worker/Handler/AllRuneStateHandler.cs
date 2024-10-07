using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AllRuneStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        var allRuneState = new AllRuneState(context.RawState);
        return new AllRuneDocument(context.Address, allRuneState);
    }
}
