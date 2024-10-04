using Libplanet.Crypto;

namespace Mimir.Worker.Exceptions;

public class StateIsNullException : Exception
{
    public StateIsNullException(
        Address stateAddress,
        Type stateType,
        Exception? innerException = null) :
        base(
            $"State is Bencodex.Types.Null for '{stateAddress}({stateType.Name})",
            innerException)
    {
    }
}
