namespace Mimir.Worker.Initializer;

public interface IInitializerManager
{
    Task WaitInitializers(CancellationToken stoppingToken);
}
