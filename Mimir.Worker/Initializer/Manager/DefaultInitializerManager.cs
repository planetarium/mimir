using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Initializer.Manager;

public class DefaultInitializerManager : IInitializerManager
{
    private readonly List<BaseInitializer> _initializers;

    public DefaultInitializerManager(TableSheetInitializer tableSheetInitializer)
    {
        _initializers =
        [
            tableSheetInitializer
        ];
    }

    public async Task WaitInitializers(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _initializers.Any(initializer => initializer.ExecuteTask is null))
        {
            await Task.Yield();
        }

        await Task.WhenAll(_initializers.Select(initializer => initializer.ExecuteTask!));
    }
}
