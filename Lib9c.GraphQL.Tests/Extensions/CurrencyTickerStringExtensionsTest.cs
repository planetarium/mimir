using Lib9c.GraphQL.Extensions;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Tests.Extensions;

public class CurrencyTickerStringExtensionsTest
{
    public static IEnumerable<object[]> CurrencyData =>
        new[]
        {
            Currencies.Mead,
            Currencies.Crystal,
            Currencies.Garage,
            Currencies.StakeRune,
            Currencies.DailyRewardRune,
            Currencies.FreyaLiberationRune,
            Currencies.FreyaBlessingRune,
            Currencies.OdinWeaknessRune,
            Currencies.OdinWisdomRune
        }
        .Select(currency => new object[] { currency });

    [Theory]
    [MemberData(nameof(CurrencyData))]
    public void ToCurrency_ShouldConvertCorrectly(Currency currency)
    {
        var currencyTicker = currency.Ticker;

        var result = currencyTicker.ToCurrency();

        Assert.Equal(currency, result);
    }
}