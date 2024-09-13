using System;
using System.Collections.Immutable;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.InputObjects;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Extensions;

public static class CurrencyInputExtensions
{
    public static Currency ToCurrency(this CurrencyInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var minters = input.Minters?.ToImmutableHashSet();
        return input.CurrencyMethodType switch
        {
#pragma warning disable CS0618 // Type or member is obsolete
            CurrencyMethodType.Legacy => Currency.Legacy(input.Ticker, input.DecimalPlaces, minters),
#pragma warning restore CS0618 // Type or member is obsolete
            CurrencyMethodType.Capped => Currency.Capped(
                input.Ticker,
                input.DecimalPlaces,
                (input.MaximumSupplyMajor!.Value, input.MaximumSupplyMinor!.Value),
                minters),
            CurrencyMethodType.Uncapped => Currency.Uncapped(input.Ticker, input.DecimalPlaces, minters),
            _ => throw new ArgumentOutOfRangeException(
                nameof(input.CurrencyMethodType),
                $"Unexpected currency type: {input.CurrencyMethodType}")
        };
    }
}
