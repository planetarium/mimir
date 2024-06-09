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

namespace Mimir.Worker;

public class BlockPoller(
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    DiffMongoDbService mongoDbStore
)
{
    private readonly Codec Codec = new();
    private readonly string MetadataCollectionId = "BlockSyncContext";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var stateGetter = new StateGetter(stateService);
        while (!cancellationToken.IsCancellationRequested)
        {
            var syncedBlockIndex = await mongoDbStore.GetLatestBlockIndex(MetadataCollectionId);
            var currentBlockIndex = await stateService.GetLatestIndex();

            long indexDifference = Math.Abs(currentBlockIndex - syncedBlockIndex);

            if (indexDifference <= 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(7000), cancellationToken);
                continue;
            }

            int limit = (int)(indexDifference > 100 ? 100 : indexDifference);

            await ProcessTransactions(syncedBlockIndex, limit, stateGetter, cancellationToken);
            await mongoDbStore.UpdateLatestBlockIndex(
                syncedBlockIndex + limit,
                MetadataCollectionId
            );
        }
    }

    private async Task ProcessTransactions(
        long syncedBlockIndex,
        int limit,
        StateGetter stateGetter,
        CancellationToken cancellationToken
    )
    {
        var operationResult = await headlessGqlClient.GetTransactions.ExecuteAsync(
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
                    await EveryBattleArenaAsync(
                        syncedBlockIndex + limit,
                        (Dictionary)actionValues,
                        stateGetter
                    );
                }
                else if (Regex.IsMatch(actionType, "^patch_table_sheet[0-9]*$"))
                {
                    await EveryPatchTableAsync((Dictionary)actionValues);
                }
            }
        }
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
        var sheetState = await stateService.GetState(sheetAddress);
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidOperationException($"Expected sheet state to be of type 'Text'.");
        }

        sheet.Set(sheetValue.Value);

        var stateData = new StateData(sheetAddress, new SheetState(sheetAddress, sheet));
        await mongoDbStore.UpsertTableSheets(stateData, sheetState.ToDotnetString());
    }

    private async Task EveryBattleArenaAsync(
        long processBlockIndex,
        Dictionary actionValues,
        StateGetter stateGetter
    )
    {
        var myAvatarAddress = new Address(actionValues["maa"]);
        var enemyAvatarAddress = new Address(actionValues["eaa"]);

        var roundData = await stateGetter.GetArenaRoundData(processBlockIndex);
        var myArenaScore = await stateGetter.GetArenaScore(roundData, myAvatarAddress);
        var myArenaInfo = await stateGetter.GetArenaInfo(roundData, myAvatarAddress);
        var enemyArenaScore = await stateGetter.GetArenaScore(roundData, enemyAvatarAddress);
        var enemyArenaInfo = await stateGetter.GetArenaInfo(roundData, enemyAvatarAddress);

        await mongoDbStore.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(myArenaScore.address, myArenaScore),
            myAvatarAddress
        );
        await mongoDbStore.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(myArenaInfo.address, myArenaInfo),
            myAvatarAddress
        );
        await mongoDbStore.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(enemyArenaScore.address, enemyArenaScore),
            enemyAvatarAddress
        );
        await mongoDbStore.UpsertStateDataAsyncWithLinkAvatar(
            new StateData(enemyArenaInfo.address, enemyArenaInfo),
            enemyAvatarAddress
        );
    }

    private static void HandleErrors(IOperationResult operationResult)
    {
        var errors = operationResult.Errors.Select(e => e.Message);
        Serilog.Log.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    }
}
