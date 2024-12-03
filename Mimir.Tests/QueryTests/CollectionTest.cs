using Mimir.MongoDB.Repositories;
using Moq;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;

namespace Mimir.Tests.QueryTests;

public class CollectionTest
{
    [Fact]
    public async Task GraphQL_Query_Collection_Returns_CorrectValue()
    {
        var address = new Address("0x0000000001000000000200000000030000000004");
        var state = new CollectionState
        {
            Ids = new SortedSet<int>(new[] { 1, 2, 3 })
        };
        var mockRepo = new Mock<ICollectionRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new CollectionDocument(1, address, state));
        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();
            var query = $$"""
                           query {
                             collection(address: "{{address}}") {
                               ids
                             }
                           }
                           """;

           var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));
            
           await Verify(result);
    }
}