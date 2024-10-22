using Lib9c;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class MeadBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseBalanceHandler("balance_mead", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<MeadBalanceHandler>(), Currencies.Mead);