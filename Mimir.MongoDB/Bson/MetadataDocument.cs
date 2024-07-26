using MongoDB.Bson;

namespace Mimir.MongoDB.Bson;

public class MetadataDocument
{
    public ObjectId Id { get; set; }
    public required string PollerType { get; set; }
    public required string CollectionName { get; set; }
    public long LatestBlockIndex { get; set; }
}
