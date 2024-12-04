using DnsClient.Protocol;
using Lib9c.Models.Stake;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

public class StakeTest
{
    [Fact]
    public async Task GraphQL_Query_Stake_Returns_CorrectValue()
    {
        var address = new Address();
        // 모킹 처리
        var mockRepo = new Mock<IStakeRepository>();
        mockRepo
            .Setup(repo => repo.GetByAgentAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(
                new StakeDocument(
                    1,
                    new Address(),
                    new Address(),
                    new StakeState() {
                        Contract = new Contract(){
                            StakeRegularFixedRewardSheetTableName = "a",
                            StakeRegularRewardSheetTableName = "a",
                            RewardInterval = 1,
                            LockupInterval = 1
                        },
                        StartedBlockIndex = 2,
                        ReceivedBlockIndex = 1
                    },
                    1
                    ));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();
        
        var query = $$"""
            query {
                stake(address: "{{address}}") {                            
                    receivedBlockIndex
                    startedBlockIndex
                    contract {
                    lockupInterval
                    rewardInterval
                    stakeRegularFixedRewardSheetTableName
                    stakeRegularRewardSheetTableName                        
                }
            }
        }
        """;

    var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

    await Verify(result);
    }
}