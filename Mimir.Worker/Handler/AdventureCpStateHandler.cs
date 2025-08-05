using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AdventureCpStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter)
    : BaseDiffHandler(
        "adventure_cp",
        Addresses.AdventureCp,
        new AdventureCpStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<AdventureCpStateHandler>());