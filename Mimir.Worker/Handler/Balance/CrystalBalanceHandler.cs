using Lib9c;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class CrystalBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseBalanceHandler("balance_crystal", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<CrystalBalanceHandler>(), Currencies.Crystal);
