using Lib9c.Models.Runes;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class RuneSlotTest
{
    [Fact]
    public async Task GraphQL_Query_RuneSlot_Returns_CorrectValue()
    {
        var address = new Address("0x0000000001000000000200000000030000000004");
        var state = new RuneSlotState
        {
            BattleType = Nekoyume.Model.EnumType.BattleType.Adventure,
            Slots = new List<RuneSlot>
            {
                new RuneSlot
                {
                    Index = 1,
                    RuneSlotType = Nekoyume.Model.EnumType.RuneSlotType.Default,
                    RuneType = Nekoyume.Model.EnumType.RuneType.Stat,
                    IsLock = false,
                    RuneId = 10
                }
            },
        };
        var mockRepo = new Mock<IRuneSlotRepository>();
        mockRepo
            .Setup(repo =>
                repo.GetByAddressAsync(
                    It.IsAny<Address>(),
                    Nekoyume.Model.EnumType.BattleType.Adventure
                )
            )
            .ReturnsAsync(new RuneSlotDocument(1, address, state));
        var serviceProvider = TestServices.Builder.With(mockRepo.Object).Build();
        var query = $$"""
            query {
              runeSlot(address: "{{address}}", battleType: ADVENTURE) {
                battleType,
                slots {
                    index
                    runeSlotType
                    runeType
                    isLock
                    runeId
                }
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
