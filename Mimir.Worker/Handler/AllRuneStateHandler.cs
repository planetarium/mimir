using Bencodex.Types;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AllRuneStateHandler : IStateHandler
{
    public IMimirBsonDocument ConvertToState(StateDiffContext context)
    {
        if (context.RawState is not List value)
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(List)}, got {context.RawState.GetType().Name}."
            );
        }

        var allRuneState = new AllRuneState(value);
        return new AllRuneDocument(context.Address, allRuneState);
    }
}
