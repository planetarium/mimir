using Lib9c;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class OdinWisdomRuneBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager)
    : BaseBalanceHandler("balance_wisdom_rune", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<OdinWisdomRuneBalanceHandler>(), Currencies.OdinWisdomRune);