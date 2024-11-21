using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Extensions;
using Lib9c.GraphQL.InputObjects;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Tests.Extensions;

public class CurrencyInputExtensionsTest
{
    [Fact]
    public void ToCurrency_ShouldConvertCorrectly()
    {
        var input = new CurrencyInput(
            CurrencyMethodType.Legacy,
            Ticker: "NCG",
            DecimalPlaces: 2,
            Minters: null,
            TotalSupplyTrackable: false,
            MaximumSupplyMajor: null,
            MaximumSupplyMinor: null);
        var expected = Currency.Legacy("NCG", 2, null);
        var result = input.ToCurrency();
        Assert.Equal(expected, result);
    }
}
