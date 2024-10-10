using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class CombinationSlotStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context) =>
        new CombinationSlotStateDocument(context.Address, new CombinationSlotState(context.RawState));
}
