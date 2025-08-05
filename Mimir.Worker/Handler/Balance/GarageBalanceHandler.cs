using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Initializer.Manager;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class GarageBalanceHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseBalanceHandler(
        "balance_garage",
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetterService,
        Log.ForContext<GarageBalanceHandler>(),
        Currencies.Garage
    );
