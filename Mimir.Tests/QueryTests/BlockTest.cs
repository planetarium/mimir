using Lib9c.Models.Block;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using MongoDB.Bson;
using Moq;

namespace Mimir.Tests.QueryTests;

public class BlockTest
{
    [Fact]
    public async Task GraphQL_Query_Block_Returns_CorrectValue()
    {
        var blockIndex = 6494625L;
        var blockHash = "348245059ff5695b67077d42a4c43327ebcd876f899e2a99de604e9db3eca04f";
        var miner = new Address("0x088d96AF8e90b8B2040AeF7B3BF7d375C9E421f7");
        var stateRootHash = "830dfa32f49e7aa8b72a792fbc7654b1aaa0b6be1a8f060648cc8a2911983249";
        var timestamp = "2025-07-07T14:16:28.289321+00:00";

        var block = new Block
        {
            Index = blockIndex,
            Hash = blockHash,
            Miner = miner,
            StateRootHash = stateRootHash,
            Timestamp = timestamp,
        };

        var mockRepo = new Mock<IBlockRepository>();
        mockRepo
            .Setup(repo => repo.GetByIndexAsync(It.IsAny<long>()))
            .ReturnsAsync(new BlockDocument(6494611, blockHash, block));

        var serviceProvider = TestServices.Builder.With(mockRepo.Object).Build();

        var query = $$"""
            query {
              block(index: {{blockIndex}}) {
                object {
                  hash
                  index
                  miner
                  stateRootHash
                  timestamp
                }
              }
            }
            """;

        var result = await TestServices.ExecuteRequestAsync(
            serviceProvider,
            b => b.SetDocument(query)
        );

        await Verify(result);
    }
}
