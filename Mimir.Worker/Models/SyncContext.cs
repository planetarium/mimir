using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Worker.Models;

public class SyncContext
{
    [BsonId]
    public string Id { get; } = "SyncContext";
    public long LatestBlockIndex { get; set; }
}
