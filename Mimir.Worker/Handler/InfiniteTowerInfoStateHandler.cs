using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class InfiniteTowerInfoStateHandler(
    int infiniteTowerId,
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter)
    : BaseDiffHandler(
        "infinite_tower_info",
        Addresses.InfiniteTowerInfo.Derive($"{infiniteTowerId}"),
        new InfiniteTowerInfoStateDocumentConverter(infiniteTowerId),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<InfiniteTowerInfoStateHandler>().ForContext("InfiniteTowerId", infiniteTowerId));
