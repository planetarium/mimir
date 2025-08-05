using Mimir.Worker.Constants;

namespace Mimir.Options;

public class DatabaseOption
{
    public const string SectionName = "Database";
    public required string ConnectionString { get; set; }
    public required PlanetType PlanetType { get; set; }
    public string? CAFile { get; set; }
}
