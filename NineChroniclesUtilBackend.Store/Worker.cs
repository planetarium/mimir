using NineChroniclesUtilBackend.Store.Scrapper;
using NineChroniclesUtilBackend.Store.Services;

namespace NineChroniclesUtilBackend.Store;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IStateService _stateService;

    public Worker(ILogger<Worker> logger, IStateService stateService)
    {
        _logger = logger;
        _stateService = stateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            var store = new ArenaScrapper(_stateService);
            await store.ExecuteAsync();

            await Task.Delay(1000, stoppingToken);
        }
    }
}
