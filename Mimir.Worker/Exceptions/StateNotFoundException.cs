using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Libplanet.Crypto;

namespace Mimir.Worker.Exceptions;

public class StateNotFoundException : Exception
{
    public StateNotFoundException(
        Address stateAddress,
        Type stateType,
        Exception? innerException = null
    )
        : base($"State not found for '{stateAddress}({stateType.Name})", innerException) { }
}
