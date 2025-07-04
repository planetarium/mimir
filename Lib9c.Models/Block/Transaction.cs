using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Block;

/// <summary>
/// <see cref="Transaction"/>
/// </summary>
[BsonIgnoreExtraElements]
public record class Transaction
{
    public List<Action> Actions { get; set; }
    public string Id { get; set; }
    public long Nonce { get; set; }
    public string PublicKey { get; set; }
    public string Signature { get; set; }
    public string Signer { get; set; }
    public string Timestamp { get; set; }
    public List<string> UpdatedAddresses { get; set; }
}
