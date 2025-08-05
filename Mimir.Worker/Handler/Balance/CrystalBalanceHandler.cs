using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Initializer.Manager;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class CrystalBalanceHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseBalanceHandler(
        "balance_crystal",
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetterService,
        Log.ForContext<CrystalBalanceHandler>(),
        Currencies.Crystal
    );
