using System.Text.Json;
using Lib9c.Models.Block;
using MongoDB.Bson;
using Xunit;
using Libplanet.Crypto;

public class TransactionTest
{
    [Fact]
    public void Transaction_Constructor_And_Properties_Works()
    {
        var tx = new Transaction
        {
            Id = "txid",
            Nonce = 1,
            PublicKey = "pubkey",
            Signature = "sig",
            Signer = new Address("0x0000000000000000000000000000000000000000"),
            Timestamp = "2024-01-01T00:00:00Z",
            UpdatedAddresses = new List<Address> { new Address("0x0000000000000000000000000000000000000001"), new Address("0x0000000000000000000000000000000000000002") },
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

        Assert.Equal("txid", tx.Id);
        Assert.Equal(1, tx.Nonce);
        Assert.Equal("pubkey", tx.PublicKey);
        Assert.Equal("sig", tx.Signature);
        Assert.Equal(new Address("0x0000000000000000000000000000000000000000"), tx.Signer);
        Assert.Equal("2024-01-01T00:00:00Z", tx.Timestamp);
        Assert.Equal(2, tx.UpdatedAddresses.Count);
        Assert.Single(tx.Actions);
        Assert.Equal("rawdata", tx.Actions[0].Raw);
        Assert.Equal("typeid", tx.Actions[0].TypeId);
        Assert.Equal("v", tx.Actions[0].Values["k"].AsString);
    }

    [Fact]
    public void Transaction_Serialization_And_Deserialization_Works()
    {
        var tx = new Transaction
        {
            Id = "txid",
            Nonce = 1,
            PublicKey = "pubkey",
            Signature = "sig",
            Signer = new Address("0x0000000000000000000000000000000000000000"),
            Timestamp = "2024-01-01T00:00:00Z",
            UpdatedAddresses = new List<Address> { new Address("0x0000000000000000000000000000000000000001"), new Address("0x0000000000000000000000000000000000000002") },
            Actions = new List<Lib9c.Models.Block.Action>(),
        };
        var json = JsonSerializer.Serialize(tx);
        var deserialized = JsonSerializer.Deserialize<Transaction>(json);
        Assert.Equal(tx.Id, deserialized.Id);
        Assert.Equal(tx.Nonce, deserialized.Nonce);
        Assert.Equal(tx.PublicKey, deserialized.PublicKey);
        Assert.Equal(tx.Signature, deserialized.Signature);
        Assert.Equal(tx.Signer, deserialized.Signer);
        Assert.Equal(tx.Timestamp, deserialized.Timestamp);
        Assert.Equal(tx.UpdatedAddresses, deserialized.UpdatedAddresses);
        Assert.NotNull(deserialized.Actions);
    }
} 