namespace Mimir.Options;

public class HeadlessEndpoint
{
    public Uri Endpoint { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtSecretKey { get; set; }
}

public class HeadlessStateServiceOption
{
    public Dictionary<string, HeadlessEndpoint> Endpoints { get; set; }
}
