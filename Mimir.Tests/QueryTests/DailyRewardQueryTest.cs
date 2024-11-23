namespace Mimir.Tests.QueryTests;

public class DailyRewardQueryTest
{
    [Fact]
    public async Task GraphQL_Query_DailyRewardReceivedBlockIndex_Returns_CorrectValue()
    {
        var serviceProvider = TestServices.CreateServices();

        var query = """
                    query {
                      dailyRewardReceivedBlockIndex(address: "0x0000000000000000000000000000000000000000")
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetQuery(query));

        await Verify(result);
    }
}