using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AllCombinationSlotStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler("all_combination_slot",
        new AllCombinationSlotStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<AllCombinationSlotStateHandler>());