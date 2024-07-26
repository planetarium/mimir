namespace Mimir.Worker.Poller;

public interface IBlockPoller
{
    public Task RunAsync(CancellationToken stoppingToken);
}
