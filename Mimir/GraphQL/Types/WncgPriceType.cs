using System.Text.Json;
using System.Text.Json.Serialization;
using HotChocolate;

namespace Mimir.GraphQL.Types;

public class UnixTimestampConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out var dateTime))
            {
                return dateTime;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var timestamp = reader.GetInt64();
            if (timestamp > 1000000000000) // 밀리초 단위
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else // 초 단위
            {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
        }

        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

public class WncgPriceType
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public int? IsActive { get; set; }

    [JsonPropertyName("is_fiat")]
    public int? IsFiat { get; set; }

    [JsonPropertyName("circulating_supply")]
    public decimal CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")]
    public decimal TotalSupply { get; set; }

    [JsonPropertyName("max_supply")]
    public decimal MaxSupply { get; set; }

    [JsonPropertyName("date_added")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTime? DateAdded { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public int? NumMarketPairs { get; set; }

    [JsonPropertyName("cmc_rank")]
    public int? CmcRank { get; set; }

    [JsonPropertyName("last_updated")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTime LastUpdated { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("platform")]
    public object? Platform { get; set; }

    [JsonPropertyName("self_reported_circulating_supply")]
    public decimal? SelfReportedCirculatingSupply { get; set; }

    [JsonPropertyName("self_reported_market_cap")]
    public decimal? SelfReportedMarketCap { get; set; }

    [JsonPropertyName("quote")]
    public WncgQuoteType Quote { get; set; } = new();
}

public class WncgQuoteType
{
    [JsonPropertyName("USD")]
    public WncgUsdQuoteType USD { get; set; } = new();
}

public class WncgUsdQuoteType
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal Volume24h { get; set; }

    [JsonPropertyName("volume_change_24h")]
    public decimal VolumeChange24h { get; set; }

    [JsonPropertyName("percent_change_1h")]
    public decimal PercentChange1h { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public decimal PercentChange24h { get; set; }

    [JsonPropertyName("percent_change_7d")]
    public decimal PercentChange7d { get; set; }

    [JsonPropertyName("percent_change_30d")]
    public decimal PercentChange30d { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("market_cap_dominance")]
    public decimal MarketCapDominance { get; set; }

    [JsonPropertyName("fully_diluted_market_cap")]
    public decimal FullyDilutedMarketCap { get; set; }

    [JsonPropertyName("last_updated")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTime LastUpdated { get; set; }
}

public class WncgPriceResponseType
{
    [JsonPropertyName("data")]
    public Dictionary<string, JsonElement> Data { get; set; } = new();

    [JsonPropertyName("status")]
    public WncgStatusType Status { get; set; } = new();
}

public class WncgStatusType
{
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("elapsed")]
    public int Elapsed { get; set; }

    [JsonPropertyName("credit_count")]
    public int CreditCount { get; set; }

    [JsonPropertyName("notice")]
    public string Notice { get; set; } = string.Empty;
}
