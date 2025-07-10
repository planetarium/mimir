using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Block;

/// <summary>
/// <see cref="Block"/>
/// </summary>
[BsonIgnoreExtraElements]
public record class Block
{
    public long Index { get; set; }
    public string Hash { get; set; }
    public Address Miner { get; set; }
    public string StateRootHash { get; set; }
    public string Timestamp { get; set; }
    public int TxCount { get; set; }
    public List<string> TxIds { get; set; }
}
