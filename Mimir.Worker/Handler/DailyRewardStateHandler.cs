using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class DailyRewardStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseDiffHandler("daily_reward",
        Addresses.DailyReward,
        new DailyRewardStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<DailyRewardStateHandler>(),
        configuration);