using Libplanet.Crypto;
using Mimir.Worker;
using Mimir.Worker.Constants;

namespace Mimir.Initializer;

public class Configuration : Mimir.Worker.Configuration
{
    public string ChainStorePath { get; init; }
    public string[] TargetAccounts { get; init; }
    public RunOptions RunOptions { get; init; }

    public Address[] GetTargetAddresses() =>
        TargetAccounts.Select(adr => new Address(adr)).ToArray();
}
