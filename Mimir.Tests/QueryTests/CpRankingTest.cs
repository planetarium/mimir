using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Data;
using Libplanet.Crypto;
using Mimir.GraphQL.Models;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Xunit;

namespace Mimir.Tests.QueryTests;

public class CpRankingTest
{
    [Fact]
    public async Task GraphQL_Query_AdventureCpRanking_Returns_CorrectValue()
    {
        var mockRepo = new Mock<ICpRepository<AdventureCpDocument>>();
        mockRepo.Setup(repo => repo.GetRanking())
            .Returns(new[]
            {
                new AdventureCpDocument(0, new Address("0x0000000000000000000000000000000000000000"), 100),
                new AdventureCpDocument(0, new Address("0x0000000000000000000000000000000000000001"), 50)
            }.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = @"
        {
            adventureCpRanking {
                items {
                    cp
                }
            }
        }";

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GraphQL_Query_MyAdventureCpRanking_Returns_CorrectValue()
    {
        var address = new Address("0x0000000000000000000000000000000000000000");
        var userDoc = new AdventureCpDocument(0, address, 100);
        
        var mockRepo = new Mock<ICpRepository<AdventureCpDocument>>();
        mockRepo.Setup(repo => repo.GetUserWithRanking(address))
            .ReturnsAsync((userDoc, 10));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $@"
        {{
            myAdventureCpRanking(address: ""{address}"") {{
                userDocument {{
                    cp
                }}
                rank
            }}
        }}";

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
} 