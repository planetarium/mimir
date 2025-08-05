using System.Collections.Immutable;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Initializer.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class TableSheetStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseActionHandler<SheetDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^patch_table_sheet[0-9]*$",
        Log.ForContext<TableSheetStateHandler>(),
        stateGetterService
    )
{
    private static readonly ImmutableArray<Type> SheetTypes =
    [
        .. typeof(ISheet)
            .Assembly.GetTypes()
            .Where(type =>
                type.Namespace is { } @namespace
                && @namespace.StartsWith($"{nameof(Nekoyume)}.{nameof(Nekoyume.TableData)}")
                && !type.IsAbstract
                && typeof(ISheet).IsAssignableFrom(type)
            ),
    ];

    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind
            );
        }

        var tableName = ((Text)actionValues["table_name"]).ToDotnetString();
        var sheetType = tableName switch
        {
            _ when tableName.StartsWith(nameof(StakeRegularRewardSheet)) =>
                typeof(StakeRegularRewardSheet),
            _ when tableName.StartsWith(nameof(StakeRegularFixedRewardSheet)) =>
                typeof(StakeRegularFixedRewardSheet),
            _ => SheetTypes.FirstOrDefault(type => type.Name == tableName),
        };
        if (sheetType == null)
        {
            throw new TypeLoadException(
                $"Unable to find a class type matching the table name '{tableName}' in the specified namespace."
            );
        }

        return await SyncSheetStateAsync(blockIndex, tableName, sheetType, stoppingToken);
    }

    public async Task<IEnumerable<WriteModel<BsonDocument>>> SyncSheetStateAsync(
        long blockIndex,
        string sheetName,
        Type sheetType,
        CancellationToken stoppingToken = default
    )
    {
        if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
        {
            Logger.Information("ItemSheet, QuestSheet is not Sheet");
            return [];
        }

        if (
            sheetType == typeof(WorldBossKillRewardSheet)
            || sheetType == typeof(WorldBossBattleRewardSheet)
        )
        {
            Logger.Information(
                "WorldBossKillRewardSheet, WorldBossBattleRewardSheet will handling later"
            );
            return [];
        }

        var sheetInstance = Activator.CreateInstance(sheetType);
        if (sheetInstance is not ISheet sheet)
        {
            throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
        }

        var sheetAddress = Addresses.TableSheet.Derive(sheetName);
        var sheetState = await StateService.GetState(sheetAddress, stoppingToken);
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidCastException($"Expected sheet state to be of type 'Text'.");
        }

        sheet.Set(sheetValue.Value);

        return
        [
            new SheetDocument(
                blockIndex,
                sheetAddress,
                sheet,
                sheetName,
                sheetState
            ).ToUpdateOneModel(),
        ];
    }
}
