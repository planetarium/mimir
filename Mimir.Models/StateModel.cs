using Libplanet.Crypto;
using Mimir.Models.Abstractions;

namespace Mimir.Models;

public class StateModel(Address address) : IStateModel
{
    public Address Address { get; } = address;
}
