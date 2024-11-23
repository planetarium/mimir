using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class DailyRewardQueryTest
{
    [Fact]
    public async Task GraphQL_Query_DailyRewardReceivedBlockIndex_Returns_CorrectValue()
    {
        var mockAddress = new Address("0x0000000000000000000000000000000000000000");
        var mockRepo = new Mock<IDailyRewardRepository>();

        mockRepo
            .Setup(repo => repo.GetByAddressAsync(mockAddress))
            .ReturnsAsync(new DailyRewardDocument(0, mockAddress, 0));

        var serviceProvider = TestServices.CreateServices(dailyRewardRepositoryMock: mockRepo);

        var query = """
                    query {
                      dailyRewardReceivedBlockIndex(address: "0x0000000000000000000000000000000000000000")
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetQuery(query));

        await Verify(result);
    }
}