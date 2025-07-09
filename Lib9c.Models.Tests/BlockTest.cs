using System.Text.Json;
using Lib9c.Models.Block;
using MongoDB.Bson;
using Xunit;

public class BlockTest
{
    [Fact]
    public void Block_Constructor_And_Properties_Works()
    {
        var tx = new Transaction
        {
            Id = "txid",
            Nonce = 1,
            PublicKey = "pubkey",
            Signature = "sig",
            Signer = "signer",
            Timestamp = "2024-01-01T00:00:00Z",
            UpdatedAddresses = new List<string> { "addr1", "addr2" },
            Actions = new List<Lib9c.Models.Block.Action>
            {
                new Lib9c.Models.Block.Action
                {
                    Raw = "rawdata",
                    TypeId = "typeid",
                    Values = new BsonDocument { { "k", "v" } },
                },
            },
        };
        var block = new Block
        {
            Index = 123,
            Hash = "blockhash",
            Miner = "miner",
            StateRootHash = "stateroot",
            Timestamp = "2024-01-01T00:00:00Z",
            Transactions = new List<Transaction> { tx },
        };

        Assert.Equal(123, block.Index);
        Assert.Equal("blockhash", block.Hash);
        Assert.Equal("miner", block.Miner);
        Assert.Equal("stateroot", block.StateRootHash);
        Assert.Equal("2024-01-01T00:00:00Z", block.Timestamp);
        Assert.Single(block.Transactions);
        Assert.Equal("txid", block.Transactions[0].Id);
        Assert.Single(block.Transactions[0].Actions);
        Assert.Equal("rawdata", block.Transactions[0].Actions[0].Raw);
        Assert.Equal("typeid", block.Transactions[0].Actions[0].TypeId);
        Assert.Equal("v", block.Transactions[0].Actions[0].Values["k"].AsString);
    }

    [Fact]
    public void Block_Serialization_And_Deserialization_Works()
    {
        var block = new Block
        {
            Index = 1,
            Hash = "h",
            Miner = "m",
            StateRootHash = "s",
            Timestamp = "t",
            Transactions = new List<Transaction>(),
        };
        var json = JsonSerializer.Serialize(block);
        var deserialized = JsonSerializer.Deserialize<Block>(json);
        Assert.Equal(block.Index, deserialized.Index);
        Assert.Equal(block.Hash, deserialized.Hash);
        Assert.Equal(block.Miner, deserialized.Miner);
        Assert.Equal(block.StateRootHash, deserialized.StateRootHash);
        Assert.Equal(block.Timestamp, deserialized.Timestamp);
        Assert.NotNull(deserialized.Transactions);
    }
}
