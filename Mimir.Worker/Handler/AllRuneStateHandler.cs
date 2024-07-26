using Bencodex;
using Bencodex.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class AllRuneStateHandler : IStateHandler
{
    public IBencodable ConvertToState(StateDiffContext context)
    {
        if (context.RawState is not List value)
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(List)}, got {context.RawState.GetType().Name}."
            );
        }

        var allRuneState = new Nekoyume.Model.State.AllRuneState(value);
        return new AllRuneState(allRuneState);
    }
}
