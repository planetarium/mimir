using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Libplanet.Crypto;

namespace Mimir.Worker.Handler;

public class DiffContext
{
    public required GetAccountDiffsResponse DiffResponse { get; set; }
    public required string CollectionName { get; set; }
    public Address AccountAddress { get; set; }
    public long TargetBlockIndex { get; set; }
}