using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AllRuneStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter)
    : BaseDiffHandler("all_rune",
        Addresses.RuneState,
        new AllRuneStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<AllRuneStateHandler>());
