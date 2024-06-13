using Bencodex.Types;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.Handler;

public class PatchTableHandler : BaseActionHandler
{
    public PatchTableHandler(IStateService stateService, MongoDbService store)
        : base(
            stateService,
            store,
            "^patch_table_sheet[0-9]*$",
            Log.ForContext<PatchTableHandler>()
        ) { }

    public override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        Dictionary actionValues
    )
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
        _logger.Information("Handle patch_table, table: {TableName} ", tableName);

        await SyncSheetStateAsync(sheetType);
    }

    public async Task SyncSheetStateAsync(Type sheetType)
    {
        if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
        {
            _logger.Information("ItemSheet, QuestSheet is not Sheet");
            return;
        }

        if (
            sheetType == typeof(WorldBossKillRewardSheet)
            || sheetType == typeof(WorldBossBattleRewardSheet)
        )
        {
            _logger.Information(
                "WorldBossKillRewardSheet, WorldBossBattleRewardSheet will handling later"
            );
            return;
        }

        var sheetInstance = Activator.CreateInstance(sheetType);
        if (sheetInstance is not ISheet sheet)
        {
            throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
        }
        var sheetAddress = Addresses.TableSheet.Derive(sheetType.Name);
        var sheetState = await _stateService.GetState(sheetAddress);
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidCastException($"Expected sheet state to be of type 'Text'.");
        }

        sheet.Set(sheetValue.Value);

        var stateData = new StateData(
            sheetAddress,
            new SheetState(sheetAddress, sheet, sheetType.Name)
        );
        await _store.UpsertTableSheets(stateData, sheetState.ToDotnetString());
    }
}
