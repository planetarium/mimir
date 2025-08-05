using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Initializer.Manager;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class NcgBalanceHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    PlanetType planetType,
    IStateGetterService stateGetterService
)
    : BaseBalanceHandler(
        "balance_ncg",
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetterService,
        Log.ForContext<NcgBalanceHandler>(),
        planetType switch
        {
            PlanetType.Odin => NCGCurrency.OdinNCGCurrency,
            PlanetType.Heimdall => NCGCurrency.HeimdallNCGCurrency,
            _ => throw new ArgumentOutOfRangeException(nameof(planetType), planetType, null),
        }
    ) { }
