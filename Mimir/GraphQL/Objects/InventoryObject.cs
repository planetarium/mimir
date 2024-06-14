using Libplanet.Crypto;

namespace Mimir.GraphQL.Objects;

public class InventoryObject(Address address)
{
    public Address Address { get; set; } = address;
}
