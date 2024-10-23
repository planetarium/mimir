namespace Mimir.Worker.Initializer.Manager;

public interface IInitializerManager
{
    Task WaitInitializers(CancellationToken stoppingToken);
}
