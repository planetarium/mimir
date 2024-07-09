namespace Mimir.Options;

public class HeadlessStateServiceOption
{
    public string Endpoint { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtSecretKey { get; set; }
}
