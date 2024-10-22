using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AllRuneStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler("all_rune",
        new AllRuneStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<AllRuneStateHandler>());