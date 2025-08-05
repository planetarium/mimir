using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class CrystalBalanceHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseBalanceHandler("balance_crystal", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<CrystalBalanceHandler>(), Currencies.Crystal, configuration);
