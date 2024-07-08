using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class StateData(Address address, IState state) : BaseData
{
    public Address Address { get; } = address;

    public IState State { get; } = state;
}
