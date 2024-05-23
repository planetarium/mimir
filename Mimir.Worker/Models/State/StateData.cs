using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class StateData : BaseData
{
    public Address Address { get; }

    public State State { get; }

    public StateData(Address address, State state)
    {
        Address = address;
        State = state;
    }
}
