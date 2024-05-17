using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Scrapper;

public class TableSheetScrapper(
    ILogger<TableSheetScrapper> logger,
    IStateService service,
    MongoDbStore store
)
{
    private readonly ILogger<TableSheetScrapper> _logger = logger;

    private readonly IStateService _stateService = service;
    private readonly MongoDbStore _store = store;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var latestBlockIndex = await service.GetLatestIndex();
        var stateGetter = _stateService.At();

        var sheetTypes = typeof(ISheet)
            .Assembly.GetTypes()
            .Where(type =>
                type.Namespace is { } @namespace
                && @namespace.StartsWith($"{nameof(Nekoyume)}.{nameof(Nekoyume.TableData)}")
                && !type.IsAbstract
                && typeof(ISheet).IsAssignableFrom(type)
            );

        foreach (var sheetType in sheetTypes)
        {
            if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
            {
                continue;
            }

            if (sheetType == typeof(WorldBossKillRewardSheet) || sheetType == typeof(WorldBossBattleRewardSheet))
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

            var sheetData = new TableSheetData(
                sheetAddress,
                sheetType.Name,
                sheet,
                sheetState.ToDotnetString(),
                ByteUtil.Hex(new Codec().Encode(sheetState))
            );
            
            await _store.InsertTableSheets(sheetData);
        }
    }
}
