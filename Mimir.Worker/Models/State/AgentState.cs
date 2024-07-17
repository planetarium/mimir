using Bencodex;
using Bencodex.Types;

namespace Mimir.Worker.Models;

public class AgentState(Nekoyume.Model.State.AgentState agentState)
    : IBencodable
{
    public Nekoyume.Model.State.AgentState Object { get; } = agentState;

    public IValue Bencoded => Object.SerializeList();
}
