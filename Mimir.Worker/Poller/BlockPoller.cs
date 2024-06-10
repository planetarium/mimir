using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using StrawberryShake;

namespace Mimir.Worker.Poller;

public class BlockPoller : BaseBlockPoller
{
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly Codec Codec = new();

    public BlockPoller(
        ILogger<BlockPoller> logger,
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService mongoDbStore
    )
        : base(logger, stateService, mongoDbStore, "BlockPoller")
    {
        _headlessGqlClient = headlessGqlClient;
    }

    protected override async Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long currentBlockIndex,
        CancellationToken stoppingToken
    )
    {
        long indexDifference = Math.Abs(currentBlockIndex - syncedBlockIndex);
        int limit = (int)(indexDifference > 100 ? 100 : indexDifference);

        await ProcessTransactions(syncedBlockIndex, limit, stoppingToken);
    }

    private async Task ProcessTransactions(
        long syncedBlockIndex,
        int limit,
        CancellationToken cancellationToken
    )
    {
        var operationResult = await _headlessGqlClient.GetTransactions.ExecuteAsync(
            syncedBlockIndex,
            limit,
            cancellationToken
        );
        if (operationResult.Data is null)
        {
            HandleErrors(operationResult);
            return;
        }

        var txs = operationResult.Data.Transaction.NcTransactions;
        if (txs is null || txs.Count == 0)
        {
            return;
        }

        List<List<string>> actionsList = txs.Select(tx =>
                tx.Actions.Select(action => action.Raw).ToList()
            )
            .ToList();

        foreach (var actions in actionsList)
        {
            foreach (var rawAction in actions)
            {
                var action = (Dictionary)Codec.Decode(Convert.FromHexString(rawAction));
                var actionType = (Text)action["type_id"];
                var actionValues = action["values"];

                if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
                {
                    await EveryBattleArenaAsync(syncedBlockIndex + limit, (Dictionary)actionValues);
                }
                else if (Regex.IsMatch(actionType, "^patch_table_sheet[0-9]*$"))
                {
                    await EveryPatchTableAsync((Dictionary)actionValues);
                }
            }
        }

        await _store.UpdateLatestBlockIndex(syncedBlockIndex + limit, _pollerType);
    }

    private async Task EveryPatchTableAsync(Dictionary actionValues)
    {
        var sheetTypes = typeof(ISheet)
            .Assembly.GetTypes()
            .Where(type =>
                type.Namespace is { } @namespace
                && @namespace.StartsWith($"{nameof(Nekoyume)}.{nameof(Nekoyume.TableData)}")
                && !type.IsAbstract
                && typeof(ISheet).IsAssignableFrom(type)
            );

        var tableName = ((Text)actionValues["table_name"]).ToDotnetString();

        var sheetType = sheetTypes.Where(type => type.Name == tableName).FirstOrDefault();

        if (sheetType == null)
        {
            throw new TypeLoadException(
                $"Unable to find a class type matching the table name '{tableName}' in the specified namespace."
            );
        }
        var sheetInstance = Activator.CreateInstance(sheetType);
        if (sheetInstance is not ISheet sheet)
        {
            throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
        }
        var sheetAddress = Addresses.TableSheet.Derive(tableName);
        var sheetState = await _stateService.GetState(sheetAddress);
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidOperationException($"Expected sheet state to be of type 'Text'.");
        }

        sheet.Set(sheetValue.Value);

        var stateData = new StateData(
            sheetAddress,
            new SheetState(sheetAddress, sheet, sheetType.Name)
        );
        await _store.UpsertTableSheets(stateData, sheetState.ToDotnetString());
    }

    private async Task EveryBattleArenaAsync(long processBlockIndex, Dictionary actionValues)
    {
        var stateGetter = new StateGetter(_stateService);
        var myAvatarAddress = new Address(actionValues["maa"]);
        var enemyAvatarAddress = new Address(actionValues["eaa"]);

        var roundData = await stateGetter.GetArenaRoundData(processBlockIndex);
        var myArenaScore = await stateGetter.GetArenaScore(roundData, myAvatarAddress);
        var myArenaInfo = await stateGetter.GetArenaInfo(roundData, myAvatarAddress);
        var enemyArenaScore = await stateGetter.GetArenaScore(roundData, enemyAvatarAddress);
        var enemyArenaInfo = await stateGetter.GetArenaInfo(roundData, enemyAvatarAddress);

        await _store.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(
                myArenaScore.Address,
                new ArenaState(
                    myArenaScore,
                    myArenaInfo,
                    roundData,
                    myArenaScore.Address,
                    myAvatarAddress
                )
            ),
            myAvatarAddress
        );
        await _store.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(
                enemyArenaScore.Address,
                new ArenaState(
                    enemyArenaScore,
                    enemyArenaInfo,
                    roundData,
                    enemyArenaScore.Address,
                    enemyAvatarAddress
                )
            ),
            enemyAvatarAddress
        );
    }

    private static void HandleErrors(IOperationResult operationResult)
    {
        var errors = operationResult.Errors.Select(e => e.Message);
        Serilog.Log.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    }
}
