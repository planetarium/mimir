namespace Mimir.Worker;

public class Configuration
{
    public string MongoDbConnectionString { get; init; }

    public string SnapshotPath { get; init; }

    public bool EnableInitializing { get; init; } = false;

    public string DatabaseName { get; set; }
    
    public Uri HeadlessEndpoint { get; set; }

    public string Network { get; set; }

    public string? JwtIssuer { get; set; }

    public string? JwtSecretKey { get; set; }
}
