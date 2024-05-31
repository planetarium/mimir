using Libplanet.Crypto;

namespace Mimir.GraphQL.Objects;

public class AvatarObject(Address address, Address? agentAddress = null, int? index = null)
{
    public Address Address { get; set; } = address;
    public Address? AgentAddress { get; set; } = agentAddress;
    public int? Index { get; set; } = index;
}
