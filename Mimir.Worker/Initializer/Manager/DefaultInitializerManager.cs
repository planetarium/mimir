namespace Mimir.Worker.Initializer.Manager;

public class DefaultInitializerManager : IInitializerManager
{
    private readonly List<BaseInitializer> _initializers;

    public DefaultInitializerManager(TableSheetInitializer tableSheetInitializer, ArenaInitializer arenaInitializer)
    {
        _initializers =
        [
            tableSheetInitializer,
            arenaInitializer
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
