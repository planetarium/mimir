namespace Mimir.Worker.Initializer.Manager;

public class BypassInitializerManager : IInitializerManager
{
    public Task WaitInitializers(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
