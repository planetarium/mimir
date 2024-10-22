using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public sealed class CollectionStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    ILogger logger)
    : BaseDiffHandler("collection",
        new CollectionStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        logger);