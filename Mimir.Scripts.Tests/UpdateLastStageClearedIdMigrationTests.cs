using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Mimir.MongoDB.Services;
using Mimir.Scripts.Migrations;
using Xunit;

namespace Mimir.Scripts.Tests;

public class UpdateLastStageClearedIdMigrationTests
{
    private readonly Mock<IMongoDbService> _mockMongoDbService;
    private readonly Mock<ILogger<UpdateLastStageClearedIdMigration>> _mockLogger;
    private readonly Mock<IMongoCollection<BsonDocument>> _mockCollection;
    private readonly UpdateLastStageClearedIdMigration _migration;

    public UpdateLastStageClearedIdMigrationTests()
    {
        _mockMongoDbService = new Mock<IMongoDbService>();
        _mockLogger = new Mock<ILogger<UpdateLastStageClearedIdMigration>>();
        _mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        
        _migration = new UpdateLastStageClearedIdMigration(_mockMongoDbService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateLastStageClearedIdAsync_WithValidData_ShouldUpdateDocuments()
    {
        var testDocuments = CreateTestDocuments();
        var mockCursor = CreateMockCursor(testDocuments);
        
        _mockMongoDbService.Setup(x => x.GetCollection<BsonDocument>("world_information"))
            .Returns(_mockCollection.Object);
        
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), 
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor);
        
        _mockCollection.Setup(x => x.UpdateOne(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(new UpdateResult.Acknowledged(1, 1, 1));

        var result = await _migration.UpdateLastStageClearedIdAsync("test_db");

        Assert.Equal(1, result);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLastStageClearedIdAsync_WithNoValidStageClearedId_ShouldNotUpdate()
    {
        var testDocuments = CreateTestDocumentsWithNoValidStageClearedId();
        var mockCursor = CreateMockCursor(testDocuments);
        
        _mockMongoDbService.Setup(x => x.GetCollection<BsonDocument>("world_information"))
            .Returns(_mockCollection.Object);
        
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), 
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor);

        var result = await _migration.UpdateLastStageClearedIdAsync("test_db");

        Assert.Equal(0, result);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessBothDatabases()
    {
        var testDocuments1 = CreateTestDocuments();
        var testDocuments2 = CreateTestDocuments();
        var mockCursor1 = CreateMockCursor(testDocuments1);
        var mockCursor2 = CreateMockCursor(testDocuments2);
        
        _mockMongoDbService.SetupSequence(x => x.GetCollection<BsonDocument>("world_information"))
            .Returns(_mockCollection.Object)
            .Returns(_mockCollection.Object);
        
        var findAsyncCall = 0;
        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), 
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                findAsyncCall++;
                return findAsyncCall == 1 ? mockCursor1 : mockCursor2;
            });
        
        _mockCollection.Setup(x => x.UpdateOne(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(new UpdateResult.Acknowledged(1, 1, 1));

        var result = await _migration.ExecuteAsync();

        Assert.Equal(2, result);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static List<BsonDocument> CreateTestDocuments()
    {
        return new List<BsonDocument>
        {
            new BsonDocument
            {
                ["_id"] = "test_id_1",
                ["Object"] = new BsonDocument
                {
                    ["WorldDictionary"] = new BsonDocument
                    {
                        ["1"] = new BsonDocument
                        {
                            ["StageClearedId"] = 5
                        },
                        ["2"] = new BsonDocument
                        {
                            ["StageClearedId"] = 10
                        }
                    }
                },
                ["Metadata"] = new BsonDocument
                {
                    ["SchemaVersion"] = 1
                }
            }
        };
    }

    private static List<BsonDocument> CreateTestDocumentsWithNoValidStageClearedId()
    {
        return new List<BsonDocument>
        {
            new BsonDocument
            {
                ["_id"] = "test_id_1",
                ["Object"] = new BsonDocument
                {
                    ["WorldDictionary"] = new BsonDocument
                    {
                        ["1"] = new BsonDocument
                        {
                            ["StageClearedId"] = -1
                        },
                        ["2"] = new BsonDocument
                        {
                            ["StageClearedId"] = -1
                        }
                    }
                },
                ["Metadata"] = new BsonDocument
                {
                    ["SchemaVersion"] = 1
                }
            }
        };
    }

    private static IAsyncCursor<BsonDocument> CreateMockCursor(List<BsonDocument> documents)
    {
        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.SetupGet(x => x.Current).Returns(documents);
        mockCursor.Setup(x => x.Dispose()).Callback(() => { });
        return mockCursor.Object;
    }
} 