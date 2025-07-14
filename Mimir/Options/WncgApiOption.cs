namespace Mimir.Options;

public class WncgApiOption
{
    public const string SectionName = "WncgApi";
    
    public string[] ApiKeys { get; set; } = Array.Empty<string>();
} 