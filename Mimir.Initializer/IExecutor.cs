namespace Mimir.Initializer;

public interface IExecutor
{
    bool ShouldRun();

    Task RunAsync(CancellationToken stoppingToken);
}