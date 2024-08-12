namespace Mimir.Options;

public class JwtOption
{
    public const string SectionName = "Jwt";
    public required string Issuer { get; set; }
    public required string Key { get; set; }
}
