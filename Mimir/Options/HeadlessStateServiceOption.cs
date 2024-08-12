namespace Mimir.Options;

public class HeadlessStateServiceOption
{
    public const string SectionName = "StateService";
    public required string Endpoint { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtSecretKey { get; set; }
}
