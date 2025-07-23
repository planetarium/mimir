using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class OdinWeaknessRuneBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseBalanceHandler("balance_weakness_rune", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<OdinWeaknessRuneBalanceHandler>(), Currencies.OdinWeaknessRune, configuration);