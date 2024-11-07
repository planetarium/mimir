using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Worker.StateDocumentConverter;

public class AddressStatePair
{
    public int BlockIndex { get; set; }
    public Address Address { get; set; }
    public IValue RawState { get; set; }
}
