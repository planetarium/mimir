using Libplanet.Crypto;

namespace Mimir.MongoDB.Models;

public class TransactionFilter
{
    public Address? Signer { get; set; }
    public Address? AvatarAddress { get; set; }
    public string? ActionTypeId { get; set; }
    public long? BlockIndex { get; set; }
    public bool? IncludeInvolvedAddress { get; set; } = false;
    public bool? IncludeInvolvedAvatarAddress { get; set; } = false;
}
