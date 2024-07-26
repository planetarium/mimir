using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Services;
using Nekoyume.Action.Loader;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class BlockPoller : IBlockPoller
{
    private readonly MongoDbService _store;

    private readonly IStateService _stateService;

    private readonly string _pollerType;

    private readonly ILogger _logger;

    private static readonly NCActionLoader ActionLoader = new();

    private readonly BaseActionHandler[] _handlers;

    private readonly IHeadlessGQLClient _headlessGqlClient;

    private readonly Codec _codec = new();

    public BlockPoller(
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService store
    )
    {
        _headlessGqlClient = headlessGqlClient;

        _handlers =
        [
            new BattleArenaHandler(stateService, store),
            new EventDungeonBattleHandler(stateService, store),
            new HackAndSlashHandler(stateService, store),
            new HackAndSlashSweepHandler(stateService, store),
            new JoinArenaHandler(stateService, store),
            new PatchTableHandler(stateService, store),
            // new ProductsHandler(stateService, store),
            new RaidHandler(stateService, store),
            new RuneSlotStateHandler(stateService, store),
            // new RaidActionHandler(stateService, stord,
        ];

        _stateService = stateService;
        _store = store;
        _pollerType = "BlockPoller";
        _logger = Log.ForContext<BlockPoller>();
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        // var started = DateTime.UtcNow;
        // _logger.Information("Start {PollerType} background service", GetType().Name);

        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     var currentBlockIndex = await _stateService.GetLatestIndex();
        //     var syncedBlockIndex = await GetSyncedBlockIndex(currentBlockIndex);

        //     _logger.Information(
        //         "Check BlockIndex synced: {SyncedBlockIndex}, current: {CurrentBlockIndex}",
        //         syncedBlockIndex,
        //         currentBlockIndex
        //     );

        //     if (syncedBlockIndex >= currentBlockIndex)
        //     {
        //         _logger.Information("Already synced, sleep");
        //         await Task.Delay(TimeSpan.FromMilliseconds(7000), stoppingToken);
        //         continue;
        //     }

        //     await ProcessBlocksAsync(syncedBlockIndex, currentBlockIndex, stoppingToken);
        // }

        // _logger.Information(
        //     "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
        //     GetType().Name,
        //     DateTime.UtcNow.Subtract(started).Minutes
        // );
    }

    // private async Task ProcessBlocksAsync(
    //     long syncedBlockIndex,
    //     long currentBlockIndex,
    //     CancellationToken stoppingToken
    // )
    // {
    //     long indexDifference = Math.Abs(currentBlockIndex - syncedBlockIndex);
    //     int limit = 1;

    //     _logger.Information(
    //         "Process block, current&sync: {CurrentBlockIndex}&{SyncedBlockIndex}, index-diff: {IndexDiff}, limit: {Limit}",
    //         currentBlockIndex,
    //         syncedBlockIndex,
    //         indexDifference,
    //         limit
    //     );

    //     await ProcessTransactions(syncedBlockIndex, limit, stoppingToken);
    // }

    // private async Task ProcessTransactions(
    //     long syncedBlockIndex,
    //     int limit,
    //     CancellationToken cancellationToken
    // )
    // {
    //     var operationResult = await _headlessGqlClient.GetTransactions.ExecuteAsync(
    //         syncedBlockIndex,
    //         limit,
    //         cancellationToken
    //     );
    //     if (operationResult.Data is null)
    //     {
    //         var errors = operationResult.Errors.Select(e => e.Message);
    //         _logger.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    //         return;
    //     }

    //     var txs = operationResult.Data.Transaction.NcTransactions;
    //     if (txs is null || txs.Count == 0)
    //     {
    //         _logger.Information("Transactions is null or empty");
    //         return;
    //     }

    //     var blockIndex = syncedBlockIndex + limit;
    //     var tuples = txs.Where(tx => tx is not null)
    //         .Select(tx =>
    //             (
    //                 Signer: new Address(tx!.Signer),
    //                 actions: tx.Actions.Where(action => action is not null)
    //                     .Select(action =>
    //                         ActionLoader.LoadAction(
    //                             blockIndex,
    //                             _codec.Decode(Convert.FromHexString(action!.Raw))
    //                         )
    //                     )
    //                     .ToList()
    //             )
    //         )
    //         .ToList();
    //     _logger.Information("GetTransaction Success, tx-count: {TxCount}", txs.Count);
    //     var tasks = new List<Task>();

    //     foreach (var (signer, actions) in tuples)
    //     {
    //         foreach (var action in actions)
    //         {
    //             var (actionType, actionPlainValueInternal) = DeconstructActionPlainValue(
    //                 action.PlainValue
    //             );
    //             foreach (var handler in _handlers)
    //             {
    //                 tasks.Add(
    //                     handler.TryHandleAction(
    //                         blockIndex,
    //                         signer,
    //                         action,
    //                         actionType,
    //                         actionPlainValueInternal
    //                     )
    //                 );
    //             }
    //         }
    //     }
    //     await Task.WhenAll(tasks);

    //     await _store.UpdateLatestBlockIndex(syncedBlockIndex + limit, _pollerType);
    // }

    // /// <summary>
    // /// Deconstructs the given action plain value.
    // /// </summary>
    // /// <param name="actionPlainValue"><see cref="Libplanet.Action.IAction.PlainValue"/></param>
    // /// <returns>
    // /// A tuple of two values: the first is the value of the "type_id" key, and the second is the value of the
    // /// "values" key.
    // /// If the given action plain value is not a dictionary, both values are null.
    // /// And if the given action plain value is a dictionary but does not contain the "type_id" or "values" key,
    // /// the value of the key is null.
    // /// "type_id": Bencodex.Types.Text or Bencodex.Types.Integer.
    // ///            (check <see cref="Nekoyume.Action.GameAction.PlainValue"/> with
    // ///            <see cref="Libplanet.Action.ActionTypeAttribute"/>)
    // /// "values": It can be any type of Bencodex.Types.
    // /// </returns>
    // private static (IValue? typeId, IValue? values) DeconstructActionPlainValue(
    //     IValue actionPlainValue
    // )
    // {
    //     if (actionPlainValue is not Dictionary actionPlainValueDict)
    //     {
    //         return (null, null);
    //     }

    //     var actionType = actionPlainValueDict.ContainsKey("type_id")
    //         ? actionPlainValueDict["type_id"]
    //         : null;
    //     var actionPlainValueInternal = actionPlainValueDict.ContainsKey("values")
    //         ? actionPlainValueDict["values"]
    //         : null;
    //     return (actionType, actionPlainValueInternal);
    // }

    // public async Task<long> GetSyncedBlockIndex(long currentBlockIndex)
    // {
    //     try
    //     {
    //         var syncedBlockIndex = await _store.GetLatestBlockIndex(_pollerType);
    //         return syncedBlockIndex;
    //     }
    //     catch (InvalidOperationException)
    //     {
    //         _logger.Information(
    //             "Metadata collection is not found, set block index to {BlockIndex} - 1",
    //             currentBlockIndex
    //         );
    //         await _store.UpdateLatestBlockIndex(currentBlockIndex - 1, _pollerType);
    //         return currentBlockIndex - 1;
    //     }
    // }
}
