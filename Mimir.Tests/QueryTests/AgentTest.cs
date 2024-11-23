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
        var serviceProvider = TestServices.CreateServices();
        
        var query = """
                    query {
                      agent(address: "0x0000000000000000000000000000000000000000") {
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