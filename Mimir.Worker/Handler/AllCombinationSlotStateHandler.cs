using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AllCombinationSlotStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context) =>
        new AllCombinationSlotStateDocument(context.Address, new AllCombinationSlotState(context.RawState));
}
