using Microsoft.Extensions.Options;

namespace NineChroniclesUtilBackend.Store;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Configuration _config;

    public Worker(ILogger<Worker> logger, IOptions<Configuration> config)
    {
        _logger = logger;
        _config = config.Value;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
