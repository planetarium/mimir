using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AllCombinationSlotStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter)
    : BaseDiffHandler("all_combination_slot",
        Addresses.CombinationSlot,
        new AllCombinationSlotStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<AllCombinationSlotStateHandler>());
