using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Initializer.Manager;

public interface IInitializerManager
{
    Task WaitInitializers(CancellationToken stoppingToken);
}
