using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class WorldInformationStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler("world_information",
        new WorldInformationStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<WorldInformationStateHandler>());