using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Lib9c;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class StakeRuneBalanceHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter
)
    : BaseBalanceHandler(
        "balance_stake_rune",
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<StakeRuneBalanceHandler>(),
        Currencies.StakeRune
    );
