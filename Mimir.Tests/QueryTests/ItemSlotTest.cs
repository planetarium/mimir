using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class ItemSlotTest
{
    [Fact]
    public async Task GraphQL_Query_ItemSlot_Returns_CorrectValue()
    {
        var address = new Address("0x0000000001000000000200000000030000000004");
        var state = new ItemSlotState
        {
            BattleType = Nekoyume.Model.EnumType.BattleType.Adventure,
            Costumes = new List<Guid> { default },
            Equipments = new List<Guid> { default }
        };
        var mockRepo = new Mock<IItemSlotRepository>();
        mockRepo
            .Setup(repo =>
                repo.GetByAddressAsync(
                    It.IsAny<Address>(),
                    Nekoyume.Model.EnumType.BattleType.Adventure
                )
            )
            .ReturnsAsync(new ItemSlotDocument(1, address, address, state));
        var serviceProvider = TestServices.Builder.With(mockRepo.Object).Build();
        var query = $$"""
            query {
              itemSlot(address: "{{address}}", battleType: ADVENTURE) {
                battleType,
                costumes,
                equipments
              }
            }
            """;

        var result = await TestServices.ExecuteRequestAsync(
            serviceProvider,
            b => b.SetDocument(query)
        );

        await Verify(result);
    }
}
