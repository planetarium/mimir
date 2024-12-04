using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

public class PledgeTest
{
    [Fact]
    public async Task GraphQL_Query_Pledge_Returns_CorrectValue()
    {
        var mockRepo = new Mock<IPledgeRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new PledgeDocument(10000, default, default, true, 10));
        
        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();
        
        var address = "0x0000008001000000000200000000030000000004";
        var query = $$"""
                    query {
                        pledge(agentAddress: "{{address}}") {
                            address
                            contractAddress
                            contracted
                            id
                            refillMead
                            storedBlockIndex
                            metadata {
                            schemaVersion
                            storedBlockIndex
                            }
                        }
                    }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);      
    }
}