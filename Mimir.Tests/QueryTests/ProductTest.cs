using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

namespace Mimir;

public class ProductTest
{
    [Fact]
    public async Task GraphQL_Query_Product_Returns_CorrectValue()
    {
        
        var address = new Address("0x0000000001000000000200000000030000000004");
        var state = new CollectionState
        {
            Ids = new SortedSet<int>(new[] { 1, 2, 3 })
        };
        var mockRepo = new Mock<IProductRepository>();
        mockRepo
            .Setup(repo => repo.GetByProductIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new ProductDocument(0, default, default, default, new Product() {
                ProductId = default,
                RegisteredBlockIndex = 0,
                SellerAvatarAddress = default,
                SellerAgentAddress = default,
            }));
        
        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();
        
        var query = $$"""
                      query {
                          product(productId: "0b9567ea-d3c0-4074-8c74-56375721c042") {
                            sellerAgentAddress
                            sellerAvatarAddress
                          }
                      }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));        

        await Verify(result);
    }
}