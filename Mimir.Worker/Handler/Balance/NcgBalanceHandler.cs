using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class NcgBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    PlanetType planetType)
    : BaseBalanceHandler("balance_ncg", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<NcgBalanceHandler>(), planetType switch
        {
            PlanetType.Odin => OdinNCGCurrency,
            PlanetType.Heimdall => HeimdallNCGCurrency,
            _ => throw new ArgumentOutOfRangeException(nameof(planetType), planetType, null)
        })
{
    private static readonly Currency OdinNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")
    );

    private static readonly Currency HeimdallNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        null
    );
}