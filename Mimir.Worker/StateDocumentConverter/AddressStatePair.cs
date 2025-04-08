using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Worker.StateDocumentConverter;

public class AddressStatePair
{
    public long BlockIndex { get; set; }
    public Address Address { get; set; }
    public IValue RawState { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}
