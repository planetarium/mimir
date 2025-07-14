using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimir.GraphQL.Types;
using Mimir.Options;

namespace Mimir.Services;

public class WncgPriceService : IWncgPriceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string[] _apiKeys;
    private readonly Random _random;
    private const string CacheKey = "wncg_price";
    private const int CacheDurationHours = 24;

    public WncgPriceService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<WncgApiOption> options
    )
    {
        _httpClient = httpClient;
        _cache = cache;
        _apiKeys = options.Value.ApiKeys;
        _random = new Random(Environment.TickCount);
    }

    public async Task<WncgPriceType?> GetWncgPriceAsync()
    {
        if (_cache.TryGetValue(CacheKey, out WncgPriceType? cachedPrice))
        {
            return cachedPrice;
        }

        var price = await FetchWncgPriceFromApiAsync();

        if (price != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                TimeSpan.FromHours(CacheDurationHours)
            );
            _cache.Set(CacheKey, price, cacheOptions);
        }

        return price;
    }

    private async Task<WncgPriceType?> FetchWncgPriceFromApiAsync()
    {
        try
        {
            var apiKey = _apiKeys.Length > 0 ? _apiKeys[_random.Next(_apiKeys.Length)] : "NO";
            var url =
                "https://pro-api.coinmarketcap.com/v2/cryptocurrency/quotes/latest?symbol=wncg&aux=max_supply,circulating_supply,total_supply&skip_invalid=true&convert=USD";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-CMC_PRO_API_KEY", apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<WncgPriceResponseType>(jsonString);

            // 실제 응답은 data["WNCG"]가 배열임
            if (
                responseData?.Data != null
                && responseData.Data.TryGetValue("WNCG", out var wncgList)
            )
            {
                if (wncgList.ValueKind == JsonValueKind.Array)
                {
                    var arr = wncgList.EnumerateArray().ToList();
                    if (arr.Count > 0)
                    {
                        var wncg = JsonSerializer.Deserialize<WncgPriceType>(
                            arr[0].GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        return wncg;
                    }
                }
                else if (wncgList.ValueKind == JsonValueKind.Object)
                {
                    var wncg = JsonSerializer.Deserialize<WncgPriceType>(
                        wncgList.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return wncg;
                }
            }
            // fallback: 기존 구조도 지원
            if (responseData?.Data?.Count > 0)
            {
                var first = responseData.Data.Values.First();
                if (first.ValueKind == JsonValueKind.Object)
                {
                    return first.Deserialize<WncgPriceType>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
