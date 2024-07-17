using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public class AgentStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context) =>
        new(context.Address, ConvertToState(context.RawState));

    private static AgentState ConvertToState(IValue state)
    {
        var agentState = state switch
        {
            List list => new Nekoyume.Model.State.AgentState(list),
            Dictionary dictionary => new Nekoyume.Model.State.AgentState(dictionary),
            _
                => throw new InvalidCastException(
                    $"{nameof(state)} Invalid state type. Expected {nameof(List)} or {nameof(Dictionary)}, got {state.GetType().Name}."
                ),
        };

        return new AgentState(agentState);
    }

    public async Task StoreStateData(MongoDbService store, StateData stateData)
    {
        await store.UpsertStateDataAsync(stateData);
    }
}
