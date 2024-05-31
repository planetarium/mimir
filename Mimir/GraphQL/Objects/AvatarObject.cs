using Libplanet.Crypto;

namespace Mimir.GraphQL.Objects;

public class AvatarObject(Address address)
{
    public Address Address { get; set; } = address;
}
