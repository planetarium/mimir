using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Constants;

public enum PollerType
{
    DiffPoller,
    TxPoller,
    BlockPoller,
}
