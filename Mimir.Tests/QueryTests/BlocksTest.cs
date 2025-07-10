using HotChocolate;
using HotChocolate.Data;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Libplanet.Crypto;

namespace Mimir.Tests.QueryTests;

public class BlocksTest
{
    [Fact]
    public async Task GetBlocks_Returns_PaginatedBlocks()
    {
        var mockRepo = new Mock<IBlockRepository>();
        var blocks = new List<BlockDocument>
        {
            new(6494611, "blockHash1", new Lib9c.Models.Block.Block
            {
                Index = 6494611,
                Hash = "blockHash1",
                Miner = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                StateRootHash = "stateRootHash1",
                Timestamp = "2024-01-01T00:00:00Z",
            }),
            new(6494610, "blockHash2", new Lib9c.Models.Block.Block
            {
                Index = 6494610,
                Hash = "blockHash2",
                Miner = new Address("0x99cAFD096f81F722ad099e154A2000dA482c0B89"),
                StateRootHash = "stateRootHash2",
                Timestamp = "2024-01-01T00:00:01Z",
            })
        };

        mockRepo
            .Setup(repo => repo.Get())
            .Returns(blocks.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                      blocks {
                        items {
                          object {
                            index
                            hash
                            miner
                            stateRootHash
                            timestamp
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
    public async Task GetBlocks_WithPagination_Returns_CorrectPage()
    {
        var mockRepo = new Mock<IBlockRepository>();
        var blocks = new List<BlockDocument>
        {
            new(6494611, "blockHash1", new Lib9c.Models.Block.Block
            {
                Index = 6494611,
                Hash = "blockHash1",
                Miner = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7"),
                StateRootHash = "stateRootHash1",
                Timestamp = "2024-01-01T00:00:00Z",
            }),
            new(6494610, "blockHash2", new Lib9c.Models.Block.Block
            {
                Index = 6494610,
                Hash = "blockHash2",
                Miner = new Address("0x99cAFD096f81F722ad099e154A2000dA482c0B89"),
                StateRootHash = "stateRootHash2",
                Timestamp = "2024-01-01T00:00:01Z",
            })
        };

        mockRepo
            .Setup(repo => repo.Get())
            .Returns(blocks.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                      blocks(skip: 0, take: 1) {
                        items {
                          object {
                            index
                            hash
                            miner
                            stateRootHash
                            timestamp
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