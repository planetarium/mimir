using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler.Balance;

public sealed class NcgBalanceHandler(
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration)
    : BaseBalanceHandler("balance_ncg", dbService, stateService, headlessGqlClient, initializerManager,
        Log.ForContext<NcgBalanceHandler>(), configuration.Value.PlanetType switch
        {
            PlanetType.Odin => OdinNCGCurrency,
            PlanetType.Heimdall => HeimdallNCGCurrency,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration.Value.PlanetType), configuration.Value.PlanetType, null)
        }, configuration)
{
    public static readonly Currency OdinNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")
    );

    public static readonly Currency HeimdallNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        null
    );
}