using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class StakeRuneBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseBalanceHandler("balance_stake_rune", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<StakeRuneBalanceHandler>(), Currencies.StakeRune, configuration);