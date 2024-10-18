using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;

namespace Mimir.Worker.Poller;

public static class PollerFactory
{
    public static IBlockPoller CreatePoller(
        PollerType pollerType,
        IStateService stateService,
        IHeadlessGQLClient headlessGQLClient,
        MongoDbService dbService
    )
    {
        return pollerType switch
        {
            PollerType.DiffPoller => new DiffPoller(stateService, headlessGQLClient, dbService),
            _
                => throw new ArgumentException(
                    $"Unsupported poller type: {pollerType}",
                    nameof(pollerType)
                ),
        };
    }
}
