using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class CollectionStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration
)
    : BaseDiffHandler(
        "collection",
        Addresses.Collection,
        new CollectionStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<CollectionStateHandler>(),
        configuration
    );
