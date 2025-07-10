using System.Text.Json;
using Lib9c.Models.Block;
using MongoDB.Bson;
using Xunit;
using Libplanet.Crypto;

public class BlockTest
{
    [Fact]
    public void Block_Constructor_And_Properties_Works()
    {
        var block = new Block
        {
            Index = 123,
            Hash = "blockhash",
            Miner = new Address("0x0000000000000000000000000000000000000000"),
            StateRootHash = "stateroot",
            Timestamp = "2024-01-01T00:00:00Z",
            TxCount = 1,
            TxIds = new List<string> { "txid" },
        };

        Assert.Equal(123, block.Index);
        Assert.Equal("blockhash", block.Hash);
        Assert.Equal(new Address("0x0000000000000000000000000000000000000000"), block.Miner);
        Assert.Equal("stateroot", block.StateRootHash);
        Assert.Equal("2024-01-01T00:00:00Z", block.Timestamp);
        Assert.Equal(1, block.TxCount);
        Assert.Single(block.TxIds);
        Assert.Equal("txid", block.TxIds[0]);
    }

    [Fact]
    public void Block_Serialization_And_Deserialization_Works()
    {
        var block = new Block
        {
            Index = 1,
            Hash = "h",
            Miner = new Address("0x0000000000000000000000000000000000000000"),
            StateRootHash = "s",
            Timestamp = "t",
            TxCount = 0,
            TxIds = new List<string>(),
        };
        var json = JsonSerializer.Serialize(block);
        var deserialized = JsonSerializer.Deserialize<Block>(json);
        Assert.Equal(block.Index, deserialized.Index);
        Assert.Equal(block.Hash, deserialized.Hash);
        Assert.Equal(block.Miner, deserialized.Miner);
        Assert.Equal(block.StateRootHash, deserialized.StateRootHash);
        Assert.Equal(block.Timestamp, deserialized.Timestamp);
        Assert.Equal(block.TxCount, deserialized.TxCount);
        Assert.NotNull(deserialized.TxIds);
    }
}
