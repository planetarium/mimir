namespace Mimir.Options;

public class DatabaseOption
{
    public string ConnectionString { get; set; }
    public string Database { get; set; }
    public string? CAFile { get; set; }
}
