using Libplanet.Crypto;

namespace Mimir.Shared.Exceptions;

public class StateNotFoundException : Exception
{
    public StateNotFoundException(
        Address stateAddress,
        Type stateType,
        Exception? innerException = null
    )
        : base($"State not found for '{stateAddress}({stateType.Name})", innerException) { }
}
