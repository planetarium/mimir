namespace Mimir.MongoDB.Models;

public class TransactionFilter
{
    public string? Signer { get; set; }
    public string? FirstAvatarAddressInActionArguments { get; set; }
    public string? FirstActionTypeId { get; set; }
    public long? BlockIndex { get; set; }
} 