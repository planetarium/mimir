using Bencodex.Types;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.TableData;
using Nekoyume.TableData.Summon;
using Nekoyume.TableData.Swap;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Initializer;

public class TableSheetInitializer(IStateService service, MongoDbService store)
    : BaseInitializer(service, store, Log.ForContext<TableSheetInitializer>())
{
    public override async Task RunAsync(CancellationToken stoppingToken)
    {
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        foreach (var sheetType in sheetTypes)
        {
            _logger.Information("Init sheet, table: {TableName} ", sheetType.Name);

            // using (var session = await _store.GetMongoClient().StartSessionAsync())
            // {
            // session.StartTransaction();

            await SyncSheetStateAsync(
                Log.ForContext<TableSheetInitializer>().ForContext<TableSheetStateHandler>(),
                _stateService,
                _store,
                1,
                sheetType.Name,
                sheetType
            );
            // session.CommitTransaction();
            // }
        }
    }

    public override async Task<bool> IsInitialized()
    {
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        var collection = _store.GetCollection(CollectionNames.GetCollectionName<SheetDocument>());
        var count = await collection.CountDocumentsAsync(new BsonDocument());
        var sheetTypesCount = sheetTypes.Count() - 8;

        return count >= sheetTypesCount;
    }

    private static async Task SyncSheetStateAsync(
        ILogger logger,
        IStateService stateService,
        MongoDbService mongoDbService,
        long blockIndex,
        string sheetName,
        Type sheetType,
        CancellationToken stoppingToken = default
    )
    {
        if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
        {
            logger.Information("ItemSheet, QuestSheet is not Sheet");
            return;
        }

        if (
            sheetType == typeof(WorldBossKillRewardSheet)
            || sheetType == typeof(WorldBossBattleRewardSheet)
            || sheetType == typeof(CostumeSummonSheet)
            || sheetType == typeof(EquipmentSummonSheet)
            || sheetType == typeof(RuneSummonSheet)
            || sheetType == typeof(SwapRateSheet)
        )
        {
            logger.Information(
                "WorldBossKillRewardSheet, WorldBossBattleRewardSheet will handling later"
            );
            return;
        }

        var sheetInstance = Activator.CreateInstance(sheetType);
        if (sheetInstance is not ISheet sheet)
        {
            throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
        }

        var sheetAddress = Addresses.TableSheet.Derive(sheetName);
        var sheetState = await stateService.GetState(sheetAddress, stoppingToken);
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidCastException($"Expected sheet state to be of type 'Text'.");
        }

        sheet.Set(sheetValue.Value);

        await mongoDbService.UpsertSheetDocumentAsync(
            CollectionNames.GetCollectionName<SheetDocument>(),
            [new SheetDocument(blockIndex, sheetAddress, sheet, sheetName, sheetState)],
            cancellationToken: stoppingToken
        );
    }
}
