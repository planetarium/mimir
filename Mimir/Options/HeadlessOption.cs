namespace Mimir.Options;

public class HeadlessOption
{
    public const string SectionName = "Headless";
    public Uri[] HeadlessEndpoints { get; init; }

    public string? JwtIssuer { get; init; }

    public string? JwtSecretKey { get; init; }
}
