using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Tests;
using Moq;

public class MetadataTest
{
    [Fact]
    public async Task GraphQL_Query_Metadata_Returns_CorrectValue()
    {
        var mockRepo = new Mock<IMetadataRepository>();
        mockRepo
            .Setup(repo => repo.GetByCollectionAsync(It.IsAny<string>()))
            .ReturnsAsync(new MetadataDocument{
                PollerType="poller-type",
                CollectionName="collection"
            });

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
        query {
            metadata(collectionName: "collection") {
                collectionName
                latestBlockIndex
                pollerType
            }
        }
        """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
}