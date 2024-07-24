using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Mimir.Models;

/// <summary>
/// <see cref="Nekoyume.Model.State.State"/>
/// </summary>
public record State : IBencodable
{
    public Address Address { get; init; }

    public virtual IValue Bencoded => new List(Address.Serialize());

    public State(IValue bencoded)
    {
        switch (bencoded)
        {
            case List l:
                Address = l[0].ToAddress();
                return;
            case Dictionary d:
                Address = d.ContainsKey(LegacyAddressKey)
                    ? d[LegacyAddressKey].ToAddress()
                    : d[AddressKey].ToAddress();
                return;
            default:
                throw new UnsupportedArgumentTypeException<ValueKind>(
                    nameof(bencoded),
                    [ValueKind.List, ValueKind.Dictionary],
                    bencoded.Kind);
        }
    }
}
