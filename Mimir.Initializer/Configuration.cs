using Libplanet.Crypto;
using Mimir.Worker.Constants;

namespace Mimir.Initializer;

public class Configuration
{
    public string MongoDbConnectionString { get; init; }
    public PlanetType PlanetType { get; init; }
    public string? MongoDbCAFile { get; init; }
    public string ChainStorePath { get; init; }
    public string[] TargetAccounts { get; init; }
    public RunOptions RunOptions { get; init; }
    public Uri[] HeadlessEndpoints { get; init; }
    public string JwtIssuer { get; init; }
    public string JwtSecretKey { get; init; }

    public Address[] GetTargetAddresses() =>
        TargetAccounts.Select(adr => new Address(adr)).ToArray();
}
