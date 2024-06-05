using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public class AllRuneStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.Address, context.RawState));

    private static AllRuneState ConvertToState(Address address, IValue state)
    {
        if (state is not List value)
        {
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}.",
                nameof(state)
            );
        }

        var allRuneState = new Nekoyume.Model.State.AllRuneState(value);
        return new AllRuneState(address, allRuneState);
    }
}
