using Bencodex.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class ActionPointStateHandler : IStateHandler
{
    public IMimirBsonDocument ConvertToState(StateDiffContext context)
    {
        if (context.RawState is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(Integer)}, got {context.RawState.GetType().Name}."
            );
        }

        return new ActionPointDocument(context.Address, value);
    }
}
