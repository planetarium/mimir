namespace Mimir.Models.Metadata;

public record MetadataResponse(
    string PollerType,
    long LatestBlockIndex
);
