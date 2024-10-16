using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RuneSlotStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler<RuneSlotDocument>(
        stateService,
        store,
        "^(battle_arena[0-9]*|event_dungeon_battle[0-9]*|hack_and_slash[0-9]*|hack_and_slash_sweep[0-9]*|join_arena[0-9]*|raid[0-9]*|unlock_rune_slot[0-9]*)$",
        Log.ForContext<RuneSlotStateHandler>())
{
    private static readonly BattleType[] BattleTypes = Enum.GetValues<BattleType>();

    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        return (await TryProcessRuneSlotStateAsync(actionPlainValue, actionType, stoppingToken))
            .Concat(await TryProcessUnlockRuneSlotAsync(actionPlainValue, actionType, session, stoppingToken));
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        IValue actionPlainValue,
        string actionType,
        CancellationToken stoppingToken = default)
    {
        if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
        {
            var action = new BattleArena();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^event_dungeon_battle[0-9]*$"))
        {
            var action = new EventDungeonBattle();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash[0-9]*$"))
        {
            var action = new HackAndSlash();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash_sweep[0-9]*$"))
        {
            var action = new HackAndSlashSweep();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            var action = new JoinArena();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^raid[0-9]*$"))
        {
            var action = new Raid();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(action, stoppingToken);
        }

        return [];
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        BattleArena action,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.myAvatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        EventDungeonBattle action,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.AvatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        HackAndSlash action,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.AvatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        HackAndSlashSweep action,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.avatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        JoinArena action,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.avatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessRuneSlotStateAsync(
        Raid action,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return [];
        }

        return await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            BattleType.Arena,
            action.AvatarAddress,
            stoppingToken);
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessUnlockRuneSlotAsync(
        IValue actionPlainValue,
        string actionType,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (Regex.IsMatch(actionType, "^unlock_rune_slot[0-9]*$"))
        {
            var action = new UnlockRuneSlot();
            action.LoadPlainValue(actionPlainValue);
            return await TryProcessUnlockRuneSlotAsync(action, session, stoppingToken);
        }

        return [];
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> TryProcessUnlockRuneSlotAsync(
        UnlockRuneSlot action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var ops = new List<WriteModel<BsonDocument>>();
        foreach (var battleType in BattleTypes)
        {
            ops.AddRange(await RuneSlotCollectionUpdater.UpdateAsync(
                StateService,
                battleType,
                action.AvatarAddress,
                stoppingToken));
        }

        return ops;
    }
}
