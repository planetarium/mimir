using Bencodex.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class DailyRewardStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToState(StateDiffContext context)
    {
        if (context.RawState is not Integer value)
        {
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(Integer)}, got {context.RawState.GetType().Name}.",
                nameof(context.RawState)
            );
        }

        return new DailyRewardDocument(context.Address, value);
    }
}
