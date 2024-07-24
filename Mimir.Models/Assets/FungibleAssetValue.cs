using System.Numerics;
using Bencodex;
using Bencodex.Types;
using Libplanet.Types.Assets;
using Mimir.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Assets;

/// <summary>
/// <see cref="Libplanet.Types.Assets.FungibleAssetValue"/>
/// </summary>
public record FungibleAssetValue : IBencodable
{
    public Currency Currency { get; init; }
    public BigInteger RawValue { get; init; }

    public FungibleAssetValue(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.List],
                bencoded.Kind);
        }

        Currency = new Currency(l[0]);
        RawValue = (Integer)l[1];
    }

    public IValue Bencoded => List.Empty
        .Add(Currency.Serialize())
        .Add((Integer) RawValue);
}
