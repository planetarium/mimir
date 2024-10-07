namespace Mimir.Options;

public class DatabaseOption
{
    public const string SectionName = "Database";
    public required string ConnectionString { get; set; }
    public required string Database { get; set; }
    public string? CAFile { get; set; }
}
