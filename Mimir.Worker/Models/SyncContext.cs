using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Worker.Models;

public class SyncContext
{
    [BsonId]
    public string Id { get; set; }
    public string PollerType { get; set;}
    public long LatestBlockIndex { get; set; }
}
