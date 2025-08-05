using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Libplanet.Types.Assets;
using Microsoft.Extensions.Options;
using Mimir.MongoDB;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter.Balance;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler.Balance;

public abstract class BaseBalanceHandler(
    string collectionName,
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService,
    ILogger logger,
    Currency currency
)
    : BaseDiffHandler(
        collectionName,
        CollectionNames.GetAccountAddress(currency),
        new BalanceStateDocumentConverter(currency),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetterService,
        logger
    );
