using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public class ActionPointStateHandler : IStateHandler<StateData>
{
    private readonly Codec _codec = new();

    public StateData ConvertToStateData(Address address, string rawState)
    {
        var bytes = Convert.FromHexString(rawState);
        var state = _codec.Decode(bytes);
        return new StateData(address, ConvertToState(address, state));
    }

    public StateData ConvertToStateData(Address address, IValue rawState) =>
        new(address, ConvertToState(address, rawState));

    private ActionPointState ConvertToState(Address address, IValue state)
    {
        if (state is Integer value)
        {
            return new ActionPointState(address, value);
        }

        throw new ArgumentException(
            "Invalid state type. Expected Integer.",
            nameof(state)
        );
    }
}
