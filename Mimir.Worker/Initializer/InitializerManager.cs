using Mimir.Worker.Services;

namespace Mimir.Worker.Initializer
{
    public class InitializerManager
    {
        private readonly List<BaseInitializer> _initializers;

        public InitializerManager(IStateService stateService, MongoDbService store)
        {
            _initializers = new List<BaseInitializer>
            {
                new TableSheetInitializer(stateService, store),
                new MarketInitializer(stateService, store)
            };
        }

        public async Task RunInitializersAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();

            foreach (var initializer in _initializers)
            {
                if (!await initializer.IsInitialized())
                {
                    tasks.Add(initializer.RunAsync(stoppingToken));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
