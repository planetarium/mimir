using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AvatarStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler("avatar",
        new AvatarStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<AvatarStateHandler>());