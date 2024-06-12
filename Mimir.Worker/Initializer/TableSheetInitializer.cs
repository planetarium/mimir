using Bencodex.Types;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Initializer;

public class TableSheetInitializer(IStateService service, MongoDbService store)
    : BaseInitializer(service, store)
{
    public override async Task RunAsync(CancellationToken stoppingToken)
    {
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        foreach (var sheetType in sheetTypes)
        {
            if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
            {
                continue;
            }

            if (
                sheetType == typeof(WorldBossKillRewardSheet)
                || sheetType == typeof(WorldBossBattleRewardSheet)
            )
            {
                // Handle later;
                continue;
            }

            var sheetAddress = Addresses.TableSheet.Derive(sheetType.Name);
            var sheetState = await _stateService.GetState(sheetAddress);
            if (sheetState is not Text sheetValue)
            {
                throw new ArgumentException(nameof(sheetType));
            }

            var sheetInstance = Activator.CreateInstance(sheetType);
            if (sheetInstance is not ISheet sheet)
            {
                throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
            }

            sheet.Set(sheetValue.Value);

            var stateData = new StateData(
                sheetAddress,
                new SheetState(sheetAddress, sheet, sheetType.Name)
            );
            await _store.UpsertTableSheets(stateData, sheetState.ToDotnetString());
        }
    }

    public override async Task<bool> IsInitialized()
    {
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        var collection = _store.GetCollection(CollectionNames.GetCollectionName<SheetState>());
        var count = await collection.CountDocumentsAsync(new BsonDocument());
        var sheetTypesCount = sheetTypes.Count() - 4;

        return count >= sheetTypesCount;
    }
}
