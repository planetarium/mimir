using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AgentStateHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseDiffHandler("agent",
        new AgentStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<AgentStateHandler>());