using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Mimir.Worker.Constants;

namespace Mimir.Worker;

public class Configuration
{
    public string? SentryDsn { get; init; }

    public PollerType PollerType { get; init; }

    public string MongoDbConnectionString { get; init; }

    public bool EnableInitializing { get; init; } = false;

    public PlanetType PlanetType { get; init; }

    public Uri[] HeadlessEndpoints { get; init; }

    public string? JwtIssuer { get; init; }

    public string? JwtSecretKey { get; init; }

    public string? MongoDbCAFile { get; init; }
}
