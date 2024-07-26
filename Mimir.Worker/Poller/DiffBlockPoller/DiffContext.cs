using HeadlessGQL;
using Libplanet.Crypto;

namespace Mimir.Worker.Poller;

public class DiffContext
{
    public required IEnumerable<IGetAccountDiffs_AccountDiffs> Diffs { get; set; }
    public required string CollectionName { get; set; }
    public Address AccountAddress { get; set; }
    public long TargetBlockIndex { get; set; }
}
