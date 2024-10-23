using Libplanet.Crypto;
using Mimir.Worker.Client;

namespace Mimir.Worker.Handler;

public class DiffContext
{
    public required GetAccountDiffsResponse DiffResponse { get; set; }
    public required string CollectionName { get; set; }
    public Address AccountAddress { get; set; }
    public long TargetBlockIndex { get; set; }
}