using Lib9c.Models.Market;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

public class ProductIdsTest
{
    [Fact]
    public async Task GraphQL_Query_ProductIds_Returns_CorrectValue()
    {
        // process mokking
        var mockRepo = new Mock<IProductsRepository>();
        mockRepo
            .Setup(repo => repo.GetByAvatarAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new ProductsStateDocument(0, default, new ProductsState(), default));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                    query {
                        productIds(avatarAddress: "0x14f4ec204c3154768f96109fc969904e29ff69b0")
                    }
                    """;

    var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

    await Verify(result);
    }
}