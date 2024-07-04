using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class AgentState(Address address, Nekoyume.Model.State.AgentState agentState)
    : State(address)
{
    public Nekoyume.Model.State.AgentState Object { get; } = agentState;

    public override IValue SerializeList()
    {
        return Object.SerializeList();
    }
}
