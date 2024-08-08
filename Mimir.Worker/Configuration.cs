using Mimir.Worker.Constants;

namespace Mimir.Worker;

public class Configuration
{
    public PollerType PollerType { get; init; }

    public string MongoDbConnectionString { get; init; }

    public bool EnableInitializing { get; init; } = false;

    public string PlanetType { get; init; }

    public Uri[] HeadlessEndpoints { get; init; }

    public string? JwtIssuer { get; init; }

    public string? JwtSecretKey { get; init; }

    public string? MongoDbCAFile { get; init; }
}
