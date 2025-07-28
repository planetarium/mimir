using Microsoft.Extensions.Logging;
using Mimir.MongoDB.Bson;
using Mimir.Scripts.Migrations;
using Xunit;

namespace Mimir.Scripts.Tests.Migrations;

public class UpdateTransactionDocumentMigrationTests
{
    [Fact]
    public void MigrationProgress_ShouldHaveCorrectDefaultValues()
    {
        var progress = new MigrationProgress();

        Assert.Equal(long.MaxValue, progress.LastProcessedBlockIndex);
        Assert.Equal(0, progress.ProcessedCount);
        Assert.True(progress.LastUpdated <= DateTime.UtcNow);
    }

    [Fact]
    public void MigrationProgress_ShouldSerializeAndDeserializeCorrectly()
    {
        var progress = new MigrationProgress
        {
            LastProcessedBlockIndex = 1000,
            ProcessedCount = 500,
            LastUpdated = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(progress);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<MigrationProgress>(json);

        Assert.Equal(progress.LastProcessedBlockIndex, deserialized.LastProcessedBlockIndex);
        Assert.Equal(progress.ProcessedCount, deserialized.ProcessedCount);
    }
} 