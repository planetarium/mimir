using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimir.GraphQL.Types;
using Mimir.Options;
using Mimir.Services;
using Xunit;

namespace Mimir.Tests;

public class WncgPriceTypeTest
{
    [Fact]
    public async Task WncgPriceService_Caches_And_Returns_Data()
    {
        var httpClient = new HttpClient(new MockHttpMessageHandler());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Microsoft.Extensions.Options.Options.Create(new WncgApiOption { ApiKeys = new[] { "TEST_KEY" } });
        var service = new WncgPriceService(httpClient, cache, options);

        var result = await service.GetWncgPriceAsync();
        Assert.NotNull(result);
        Assert.Equal(11222, result.Id);
        Assert.Equal("Nine Chronicles", result.Name);
        Assert.Equal("WNCG", result.Symbol);
        Assert.Equal("wrapped-ncg", result.Slug);
        Assert.Equal(1000000000, result.MaxSupply);
        Assert.Equal(523397919.27m, (decimal)result.CirculatingSupply);
        Assert.Equal(723789440, result.TotalSupply);
        Assert.Equal(462425598.15m, (decimal?)result.SelfReportedCirculatingSupply);
        Assert.Equal(9707226.608314617m, (decimal?)result.SelfReportedMarketCap);
        Assert.NotNull(result.Quote);
        Assert.NotNull(result.Quote.USD);
        Assert.Equal(0.020991975027225506m, result.Quote.USD.Price);
        Assert.Equal(463831.33695151m, result.Quote.USD.Volume24h);
        Assert.Equal(-39.3913m, result.Quote.USD.VolumeChange24h);
        Assert.Equal(-0.06144031m, result.Quote.USD.PercentChange1h);
        Assert.Equal(0.41425793m, result.Quote.USD.PercentChange24h);
        Assert.Equal(6.42682908m, result.Quote.USD.PercentChange7d);
        Assert.Equal(-20.30292065m, result.Quote.USD.PercentChange30d);
        Assert.Equal(10987156.050617632m, result.Quote.USD.MarketCap);
        Assert.Equal(0, result.Quote.USD.MarketCapDominance);
        Assert.Equal(20991975.03m, result.Quote.USD.FullyDilutedMarketCap);
    }

    [Fact]
    public async Task WncgPriceService_Returns_Cached_Data_On_Second_Call()
    {
        var httpClient = new HttpClient(new MockHttpMessageHandler());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Microsoft.Extensions.Options.Options.Create(new WncgApiOption { ApiKeys = new[] { "TEST_KEY" } });
        var service = new WncgPriceService(httpClient, cache, options);

        var result1 = await service.GetWncgPriceAsync();
        var result2 = await service.GetWncgPriceAsync();
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Id, result2.Id);
        Assert.Equal(result1.Quote.USD.Price, result2.Quote.USD.Price);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
  ""status"": {
    ""timestamp"": 1720927620760,
    ""error_code"": 0,
    ""error_message"": null,
    ""elapsed"": 1096,
    ""credit_count"": 1,
    ""notice"": null
  },
  ""data"": {
    ""WNCG"": [
      {
        ""id"": 11222,
        ""name"": ""Nine Chronicles"",
        ""symbol"": ""WNCG"",
        ""slug"": ""wrapped-ncg"",
        ""max_supply"": 1000000000,
        ""circulating_supply"": 523397919.27,
        ""total_supply"": 723789440,
        ""infinite_supply"": false,
        ""self_reported_circulating_supply"": 462425598.15,
        ""self_reported_market_cap"": 9707226.608314617,
        ""tvl_ratio"": null,
        ""last_updated"": 1720927500000,
        ""quote"": {
          ""USD"": {
            ""price"": 0.020991975027225506,
            ""volume_24h"": 463831.33695151,
            ""volume_change_24h"": -39.3913,
            ""percent_change_1h"": -0.06144031,
            ""percent_change_24h"": 0.41425793,
            ""percent_change_7d"": 6.42682908,
            ""percent_change_30d"": -20.30292065,
            ""percent_change_60d"": -22.00416613,
            ""percent_change_90d"": 15.79346477,
            ""market_cap"": 10987156.050617632,
            ""market_cap_dominance"": 0,
            ""fully_diluted_market_cap"": 20991975.03,
            ""tvl"": null,
            ""last_updated"": 1720927500000
          }
        }
      }
    ]
  }
}")
            };
            return Task.FromResult(response);
        }
    }
} 