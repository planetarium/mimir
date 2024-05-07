namespace Mimir.Options;

public class HeadlessStateServiceOption
{
    public Uri HeadlessEndpoint { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtSecretKey { get; set; }
}
