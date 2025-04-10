using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Data;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Models;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Xunit;

namespace Mimir.Tests.QueryTests;

public class WorldInformationRankingTest
{
    [Fact]
    public async Task GraphQL_Query_WorldInformationRanking_Returns_CorrectValue()
    {
        var mockRepo = new Mock<IWorldInformationRankingRepository>();
        mockRepo.Setup(repo => repo.GetRanking())
            .Returns(new[]
            {
                new WorldInformationDocument(0, new Address("0x0000000000000000000000000000000000000000"), 100, new WorldInformationState()),
                new WorldInformationDocument(0, new Address("0x0000000000000000000000000000000000000001"), 50, new WorldInformationState())
            }.AsQueryable().AsExecutable());

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = @"
        {
            worldInformationRanking {
                items {
                    lastStageClearedId
                }
            }
        }";

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GraphQL_Query_MyWorldInformationRanking_Returns_CorrectValue()
    {
        var address = "0x0000000000000000000000000000000000000000";
        var userDoc = new WorldInformationDocument(0, new Address(address), 100, new WorldInformationState());
        
        var mockRepo = new Mock<IWorldInformationRankingRepository>();
        mockRepo.Setup(repo => repo.GetUserWithRanking(address))
            .ReturnsAsync((userDoc, 10));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $@"
        {{
            myWorldInformationRanking(address: ""{address}"") {{
                userDocument {{
                    lastStageClearedId
                }}
                rank
            }}
        }}";

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
} 