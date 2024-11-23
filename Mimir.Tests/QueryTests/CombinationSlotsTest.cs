using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class CombinationSlotsTest
{
    [Fact]
    public async Task GraphQL_Query_CombinationSlots_Returns_CorrectValue()
    {
        var allCombinationSlotAddress = new Address("0x0000000001000000000200000000030000000004");
        var allCombinationSlotState = new AllCombinationSlotState
        {
            CombinationSlots = new Dictionary<int, CombinationSlotState>
            {
                { 0, new CombinationSlotState { Index = 0 } },
                { 1, new CombinationSlotState { Index = 1 } },
            }
        };
        var mockRepo = new Mock<IAllCombinationSlotStateRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new AllCombinationSlotStateDocument(1, allCombinationSlotAddress, allCombinationSlotState));

        var serviceProvider = TestServices.CreateServices(
            allCombinationSlotStateRepositoryMock : mockRepo
        );

    var query = $$"""
                      query {
                        combinationSlots(avatarAddress: "{{allCombinationSlotAddress}}") {
                          key
                          value {
                            address
                            index
                            isUnlocked
                            petId
                            startBlockIndex
                            unlockBlockIndex
                            result {
                              tradableFungibleItemCount
                              typeId
                              costume {
                                elementalType
                                equipped
                                grade
                                id
                                itemId
                                itemSubType
                                itemType
                                requiredBlockIndex
                                spineResourcePath
                              }
                              itemUsable {
                                elementalType
                                grade
                                id
                                itemId
                                itemSubType
                                itemType
                                requiredBlockIndex
                              }
                              tradableFungibleItem {
                                elementalType
                                grade
                                id
                                itemId
                                itemSubType
                                itemType
                                requiredBlockIndex
                              }
                            }
                          }
                        }
                      }                 
                    """;
        var result = TestServices.ExecuteRequestAsync(
            serviceProvider,
            b => b.SetQuery(query)
        );

        await Verify(result);
    }
}