using HotChocolate;
using HotChocolate.Data;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Models;
using Mimir.MongoDB.Repositories;
using Moq;
using Libplanet.Crypto;
using MongoDB.Bson;
using Mimir.GraphQL.Queries;

namespace Mimir.Tests.QueryTests;

public class TransactionTest
{
    [Fact]
    public async Task GetTransactions_Returns_PaginatedTransactions()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                "actionType1",
                new Address("0x0000000000000000000000000000000000000001"),
                "0.01",
                new Address("0x0000000000000000000000000000000000000001"),
                new Address("0x0000000000000000000000000000000000000001"),
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = "actionType1",
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            ),
            new TransactionDocument(
                6494624,
                "txid2",
                "blockHash2",
                6494624,
                "actionType2",
                new Address("0x0000000000000000000000000000000000000002"),
                null,
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid2",
                    Nonce = 2,
                    PublicKey = "pubkey2",
                    Signature = "sig2",
                    Signer = new Address("0x99cAFD096f81F722ad099e154A2000dA482c0B89"),
                    Timestamp = "2024-01-01T00:00:01Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw2",
                            TypeId = "actionType2",
                            Values = new BsonDocument { { "amount", "200" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                      transactions {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GetTransactions_WithPagination_Returns_CorrectPage()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                "actionType1",
                new Address("0x0000000000000000000000000000000000000001"),
                "0.01",
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = "actionType1",
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            ),
            new TransactionDocument(
                6494624,
                "txid2",
                "blockHash2",
                6494624,
                "actionType2",
                new Address("0x0000000000000000000000000000000000000002"),
                null,
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid2",
                    Nonce = 2,
                    PublicKey = "pubkey2",
                    Signature = "sig2",
                    Signer = new Address("0x99cAFD096f81F722ad099e154A2000dA482c0B89"),
                    Timestamp = "2024-01-01T00:00:01Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw2",
                            TypeId = "actionType2",
                            Values = new BsonDocument { { "amount", "200" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                      transactions(skip: 0, take: 1) {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GetTransactions_WithSignerFilter_Returns_CorrectTransactions()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var signerAddress = new Address("0x0000000000000000000000000000000000000001");
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                "actionType1",
                new Address("0x0000000000000000000000000000000000000001"),
                "0.01",
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = signerAddress,
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = "actionType1",
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                    query {
                      transactions(filter: { signer: "{{signerAddress.ToHex()}}" }) {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GetTransactions_WithFirstAvatarAddressFilter_Returns_CorrectTransactions()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var avatarAddress = new Address("0x0000000000000000000000000000000000000002");
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                "actionType1",
                avatarAddress,
                "0.01",
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = "actionType1",
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                    query {
                      transactions(filter: { firstAvatarAddressInActionArguments: "{{avatarAddress.ToHex()}}" }) {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GetTransactions_WithFirstActionTypeIdFilter_Returns_CorrectTransactions()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var actionTypeId = "actionType1";
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                actionTypeId,
                new Address("0x0000000000000000000000000000000000000001"),
                "0.01",
                null,
                null,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = actionTypeId,
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                    query {
                      transactions(filter: { firstActionTypeId: "{{actionTypeId}}" }) {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GetActionTypesAsync_Returns_AlphabeticallySorted()
    {
        var mockRepo = new Mock<IActionTypeRepository>();
        var actionTypes = new List<ActionTypeDocument>
        {
            new ActionTypeDocument("zeta"),
            new ActionTypeDocument("alpha"),
            new ActionTypeDocument("beta"),
        };
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(actionTypes);
        var query = new Mimir.GraphQL.Queries.Query();
        var result = await query.GetActionTypesAsync(mockRepo.Object);
        var ids = result.Select(x => x.Id).ToList();
        Assert.Equal(new List<string> { "alpha", "beta", "zeta" }, ids);
    }

    [Fact]
    public async Task GetTransactions_WithNewFields_Returns_CorrectData()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var recipientAddress = new Address("0x0000000000000000000000000000000000000003");
        var senderAddress = new Address("0x0000000000000000000000000000000000000004");
        var transactions = new List<TransactionDocument>
        {
            new TransactionDocument(
                6494625,
                "txid1",
                "blockHash1",
                6494625,
                "transfer_asset",
                new Address("0x0000000000000000000000000000000000000001"),
                "10.5",
                recipientAddress,
                senderAddress,
                new Lib9c.Models.Block.Transaction
                {
                    Id = "txid1",
                    Nonce = 1,
                    PublicKey = "pubkey1",
                    Signature = "sig1",
                    Signer = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                    Timestamp = "2024-01-01T00:00:00Z",
                    UpdatedAddresses = new List<Address>(),
                    Actions = new List<Lib9c.Models.Block.Action>
                    {
                        new Lib9c.Models.Block.Action
                        {
                            Raw = "raw1",
                            TypeId = "transfer_asset",
                            Values = new BsonDocument { { "amount", "100" } }
                        }
                    }
                }
            )
        };

        mockRepo
            .Setup(repo => repo.Get(It.IsAny<TransactionFilter>()))
            .Returns(transactions.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                      transactions {
                        items {
                          id
                          blockHash
                          blockIndex
                          firstActionTypeId
                          firstAvatarAddressInActionArguments
                          firstNCGAmountInActionArguments
                          firstRecipientInActionArguments
                          firstSenderInActionArguments
                          object {
                            id
                            nonce
                            publicKey
                            signature
                            signer
                            timestamp
                            updatedAddresses
                            actions {
                              raw
                              typeId
                              values
                            }
                          }
                        }
                        pageInfo {
                          hasNextPage
                          hasPreviousPage
                        }
                      }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
} 