using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public class AgentStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.Address, context.RawState));

    private static AgentState ConvertToState(Address address, IValue state)
    {
        var agentState = state switch
        {
            List list => new Nekoyume.Model.State.AgentState(list),
            Dictionary dictionary => new Nekoyume.Model.State.AgentState(dictionary),
            _ => throw new ArgumentException(
                $"Invalid state type. Expected {nameof(List)} or {nameof(Dictionary)}, got {state.GetType().Name}.",
                nameof(state)
            ),
        };

        return new AgentState(address, agentState);
    }
}
