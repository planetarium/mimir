using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

public class PetTest
{
    [Fact]
    public async Task GraphQL_Query_Pet_Returns_CorrectValue()
    {

        // process mokking
        var address = new Address();
        var mockRepo = new Mock<IPetRepository>();
        mockRepo
            .Setup(repo => repo.GetByAvatarAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new PetStateDocument(
                1,
                new Address(),
                new Address(),
                new PetState() {
                    PetId = 1,
                    Level = 1,
                    UnlockedBlockIndex = 1
                }
            ));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                    query {
                    pet(avatarAddress: "{{address}}") {
                        level
                        petId
                        UnlockedBlockIndex
                    }
                    }
                    """;

    var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

    await Verify(result);


    }
}
