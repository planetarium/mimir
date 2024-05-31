using Libplanet.Crypto;

namespace Mimir.GraphQL.Objects;

public class AgentObject(Address address)
{
    public Address Address { get; set; } = address;
}
