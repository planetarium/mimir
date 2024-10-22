using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Worker.StateDocumentConverter;

public class AddressStatePair
{
    public Address Address { get; set; }
    public IValue RawState { get; set; }
}