using Libplanet.Crypto;

namespace Mimir.MongoDB.Models;

public class TransactionFilter
{
    public Address? Signer { get; set; }
    public Address? FirstAvatarAddressInActionArguments { get; set; }
    public string? FirstActionTypeId { get; set; }
    public long? BlockIndex { get; set; }
} 