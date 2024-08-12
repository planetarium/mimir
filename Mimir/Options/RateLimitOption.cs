namespace Mimir.Options;

public class RateLimitOption
{
    public const string SectionName = "RateLimit";
    public int PermitLimit { get; set; } = 50;
    public int Window { get; set; } = 10;
    public int ReplenishmentPeriod { get; set; } = 10;
    public int QueueLimit { get; set; } = 2;
    public int SegmentsPerWindow { get; set; } = 8;
    public int TokenLimit { get; set; } = 50;
    public int TokensPerPeriod { get; set; } = 50;
    public bool AutoReplenishment { get; set; } = true;
}
