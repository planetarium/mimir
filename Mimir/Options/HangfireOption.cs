namespace Mimir.Options;

public class HangfireOption
{
    public const string SectionName = "Hangfire";
    
    public string RedisConnectionString { get; set; } = string.Empty;
    public string DashboardPath { get; set; } = "/hangfire";
    public int WorkerCount { get; set; } = Environment.ProcessorCount;
    public int CacheExpirationDays { get; set; } = 7;
    
    // Redis connection settings
    public string RedisHost { get; set; } = "localhost";
    public int RedisPort { get; set; } = 6379;
    public string RedisUsername { get; set; } = string.Empty;
    public string RedisPassword { get; set; } = string.Empty;
    public int RedisDatabase { get; set; } = 0;
    public string RedisPrefix { get; set; } = "hangfire:";
    
    // Dashboard authentication
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 