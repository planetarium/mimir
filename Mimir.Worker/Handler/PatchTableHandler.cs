using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;

public class PatchTableHandler : BaseActionHandler
{
    public PatchTableHandler(IStateService stateService, MongoDbService store)
        : base(stateService, store, "^patch_table_sheet[0-9]*$") { }

    public override async Task HandleAction(long processBlockIndex, Dictionary actionValues)
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
}
