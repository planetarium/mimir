namespace Mimir.Store;

public class Configuration
{
    public string MongoDbConnectionString { get; init; }

    public string DatabaseName { get; set; }
    
    public Uri HeadlessEndpoint { get; set; }

    public string? JwtIssuer { get; set; }

    public string? JwtSecretKey { get; set; }
}
