using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class RaidCpStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseDiffHandler(
        "raid_cp",
        Addresses.RaidCp,
        new RaidCpStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<RaidCpStateHandler>(),
        configuration);