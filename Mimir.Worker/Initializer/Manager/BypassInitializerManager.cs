using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Initializer.Manager;

public class BypassInitializerManager : IInitializerManager
{
    public Task WaitInitializers(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
