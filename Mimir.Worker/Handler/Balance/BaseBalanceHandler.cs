using Libplanet.Types.Assets;
using Mimir.MongoDB;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter.Balance;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler.Balance;

public abstract class BaseBalanceHandler(
    string collectionName,
    MongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    ILogger logger,
    Currency currency)
    : BaseDiffHandler(
        collectionName,
        CollectionNames.GetAccountAddress(currency),
        new BalanceStateDocumentConverter(currency),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        logger);
