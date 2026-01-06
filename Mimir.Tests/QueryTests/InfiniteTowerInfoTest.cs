using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Nekoyume;
using Nekoyume.Action;

namespace Mimir.Tests.QueryTests;

public class InfiniteTowerInfoTest
{
    [Fact]
    public async Task GraphQL_Query_InfiniteTowerInfo_Returns_CorrectValue()
    {
        // Arrange
        var avatarAddress = new Address("0x1234567890123456789012345678901234567890");
        var infiniteTowerId = 1;
        var accountAddress = Addresses.InfiniteTowerInfo.Derive($"{infiniteTowerId}");

        var infiniteTowerInfo = new InfiniteTowerInfo
        {
            Address = avatarAddress,
            InfiniteTowerId = infiniteTowerId,
            ClearedFloor = 10,
            RemainingTickets = 5,
            TotalTicketsUsed = 15,
            NumberOfTicketPurchases = 2,
            LastResetBlockIndex = 1000,
            LastTicketRefillBlockIndex = 2000,
        };

        var document = new InfiniteTowerInfoDocument(
            1000L,
            accountAddress,
            avatarAddress,
            infiniteTowerId,
            infiniteTowerInfo
        );

        var mockRepo = new Mock<IInfiniteTowerInfoRepository>();
        mockRepo
            .Setup(repo => repo.GetByAvatarAddressAndTowerIdAsync(
                It.IsAny<Address>(),
                It.IsAny<int>()
            ))
            .ReturnsAsync(document);

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                      query {
                        infiniteTowerInfo(avatarAddress: "{{avatarAddress}}", infiniteTowerId: {{infiniteTowerId}}) {
                          address
                          infiniteTowerId
                          clearedFloor
                          remainingTickets
                          totalTicketsUsed
                          numberOfTicketPurchases
                          lastResetBlockIndex
                          lastTicketRefillBlockIndex
                        }
                      }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
}
