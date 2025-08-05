using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RuneSlotStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService,
    PlanetType planetType
)
    : BaseActionHandler<RuneSlotDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^(battle_arena[0-9]*|event_dungeon_battle[0-9]*|hack_and_slash[0-9]*|hack_and_slash_sweep[0-9]*|join_arena[0-9]*|raid[0-9]*|unlock_rune_slot[0-9]*)$",
        Log.ForContext<RuneSlotStateHandler>(),
        stateGetterService
    )
{
    private static readonly BattleType[] BattleTypes = Enum.GetValues<BattleType>();

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
        return (
            await TryProcessRuneSlotStateAsync(
                blockIndex,
                actionPlainValue,
                actionType,
                stoppingToken
            )
        ).Concat(
            await TryProcessUnlockRuneSlotAsync(
                blockIndex,
                actionPlainValue,
                actionType,
                session,
                stoppingToken
            )
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        IValue actionPlainValue,
        string actionType,
        CancellationToken stoppingToken = default
    )
    {
        if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
        {
            var action = new BattleArena();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^event_dungeon_battle[0-9]*$"))
        {
            var action = new EventDungeonBattle();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash[0-9]*$"))
        {
            var action = new HackAndSlash();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash_sweep[0-9]*$"))
        {
            var action = new HackAndSlashSweep();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            var action = new JoinArena();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^raid[0-9]*$"))
        {
            var action = new Raid();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(blockIndex, action, stoppingToken);
        }

        return [];
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        BattleArena action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Arena,
            action.myAvatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        EventDungeonBattle action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Adventure,
            action.AvatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        HackAndSlash action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Adventure,
            action.AvatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        HackAndSlashSweep action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Adventure,
            action.avatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        JoinArena action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Arena,
            action.avatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        long blockIndex,
        Raid action,
        CancellationToken stoppingToken = default
    )
    {
        return await RuneSlotCollectionUpdater.UpdateAsync(
            blockIndex,
            StateService,
            BattleType.Raid,
            action.AvatarAddress,
            stoppingToken
        );
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessUnlockRuneSlotAsync(
        long blockIndex,
        IValue actionPlainValue,
        string actionType,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (Regex.IsMatch(actionType, "^unlock_rune_slot[0-9]*$"))
        {
            var action = new UnlockRuneSlot();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessUnlockRuneSlotAsync(blockIndex, action, session, stoppingToken);
        }

        return [];
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessUnlockRuneSlotAsync(
        long blockIndex,
        UnlockRuneSlot action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var ops = new List<WriteModel<BsonDocument>>();
        foreach (var battleType in BattleTypes)
        {
            ops.AddRange(
                await RuneSlotCollectionUpdater.UpdateAsync(
                    blockIndex,
                    StateService,
                    battleType,
                    action.AvatarAddress,
                    stoppingToken
                )
            );
        }

        return ops;
    }
}
