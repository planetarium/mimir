using Bencodex.Types;
using Lib9c.Models.States;
using Lib9c.Models.WorldInformation;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;
using Nekoyume.Model.State;

public class WorldInformationTest
{
    [Fact]
    public async Task GraphQL_Query_WorldInformation_Returns_CorrectValue()
    {
        var address = new Address("0x0000000000000000000000000000000000000000");

        var mockRepo = new Mock<IWorldInformationRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new WorldInformationDocument(
                0,
                default,
                new WorldInformationState()
                {
                    WorldDictionary = new Dictionary<int, World>()
                    {
                        { 
                            1, 
                            new World(new Dictionary(new Dictionary<IKey, IValue>
                            {
                                [(Text)"Id"] = 1.Serialize(), 
                                [(Text)"Name"] = "kim".Serialize(), 
                                [(Text)"StageBegin"] = 2.Serialize(),
                                [(Text)"StageEnd"] = 3.Serialize(),
                                [(Text)"UnlockedBlockIndex"] = 2.Serialize(),
                                [(Text)"StageClearedBlockIndex"] = 1.Serialize(),
                                [(Text)"StageClearedId"] = 2.Serialize(),
                            }))
                        }
                    }
                }
            ));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
            query {
                worldInformation(address: "{{address}}") {
                    worldDictionary {
                        key
                        value {
                            id
                            isStageCleared
                            isUnlocked
                            name
                            stageBegin
                            stageClearedBlockIndex
                            stageClearedId
                            stageEnd
                            unlockedBlockIndex
                        }
                    }
                }
            }
            """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
}
