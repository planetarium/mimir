using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Nekoyume;

namespace Mimir.Tests.QueryTests;

public class AgentTest
{
    [Fact]
    public async Task GraphQl_Query_Agent_Returns_CorrectValue()
    {
        var agentAddress = new Address("0x0000000031000000000200000000030000000004");
        var avatarAddress1 = new Address("0x0000005001000000000200000000030000000004");
        var avatarAddress2 = new Address("0x0000008001000000000200000000030000000004");
        var avatarAddress3 = new Address("0x0000001001000000000200000000030000000004");
        var agentState = new AgentState
        {
            Address = agentAddress,
            AvatarAddresses = new Dictionary<int, Address>
            {
                { 0, avatarAddress1 },
                { 1, avatarAddress2 },
                { 2, avatarAddress3 }
            },
            MonsterCollectionRound = 3,
            Version = 4,
        };
        var mockRepo = new Mock<IAgentRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new AgentDocument(1, agentAddress, agentState));

        var serviceProvider = new TestServices.ServiceProviderBuilder()
            .With(mockRepo.Object)
            .Build();
        var query = $$"""
                      query {
                        agent(address: "{{agentAddress}}") {
                          address
                          monsterCollectionRound
                          version
                          avatarAddresses {
                            key
                            value
                          }
                        }
                      }
                      """;
        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetQuery(query));

        await Verify(result);
    }
}
