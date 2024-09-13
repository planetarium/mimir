using System;
using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Extensions;

public static class CurrencyTickerStringExtensions
{
#pragma warning disable CS0618 // Type or member is obsolete
    private static readonly Currency OdinNCG = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde"));
#pragma warning restore CS0618 // Type or member is obsolete

    public static Currency ToCurrency(this string currencyTicker)
    {
        if (currencyTicker is null)
        {
            throw new ArgumentNullException(nameof(currencyTicker));
        }

        return currencyTicker switch
        {
            "NCG" => OdinNCG,
            "Mead" => Currencies.Mead,
            _ => Currencies.GetMinterlessCurrency(currencyTicker),
        };
    }
}
