namespace Mimir.MongoDB.Bson;

public record class DocumentMetadata(int SchemaVersion, long StoredBlockIndex) { }
