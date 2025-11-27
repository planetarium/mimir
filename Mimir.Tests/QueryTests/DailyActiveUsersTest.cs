using Mimir.MongoDB.Models;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class DailyActiveUsersTest
{
    [Fact]
    public async Task GraphQL_Query_DailyActiveUsers_Returns_CorrectValue()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo
            .Setup(repo => repo.GetDailyActiveUsersAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<DailyActiveUser>
            {
                new DailyActiveUser("2024-01-01", 100),
                new DailyActiveUser("2024-01-02", 150),
                new DailyActiveUser("2024-01-03", 120)
            });

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                        dailyActiveUsers(startDate: "2024-01-01T00:00:00Z", endDate: "2024-01-03T23:59:59Z") {
                            date
                            count
                        }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GraphQL_Query_DailyActiveUsers_WithoutDateRange_Returns_CorrectValue()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo
            .Setup(repo => repo.GetDailyActiveUsersAsync(null, null))
            .ReturnsAsync(new List<DailyActiveUser>
            {
                new DailyActiveUser("2024-01-01", 100),
                new DailyActiveUser("2024-01-02", 150)
            });

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = """
                    query {
                        dailyActiveUsers {
                            date
                            count
                        }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
}

