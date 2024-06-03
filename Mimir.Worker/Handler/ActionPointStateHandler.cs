using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public class ActionPointStateHandler : IStateHandler<StateData>
{
    private readonly Codec _codec = new();

    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.Address, context.RawState));

    private ActionPointState ConvertToState(Address address, IValue state)
    {
        if (state is Integer value)
        {
            return new ActionPointState(address, value);
        }

        throw new ArgumentException(
            $"Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}.",
            nameof(state)
        );
    }
}
