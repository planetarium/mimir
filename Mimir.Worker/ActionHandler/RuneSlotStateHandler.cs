using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RuneSlotStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^(battle_arena[0-9]*|event_dungeon_battle[0-9]*|hack_and_slash[0-9]*|hack_and_slash_sweep[0-9]*|join_arena[0-9]*|raid[0-9]*|unlock_rune_slot[0-9]*)$",
        Log.ForContext<RuneSlotStateHandler>())
{
    private static readonly BattleArena BattleArena = new();
    private static readonly EventDungeonBattle EventDungeonBattle = new();
    private static readonly HackAndSlash HackAndSlash = new();
    private static readonly HackAndSlashSweep HackAndSlashSweep = new();
    private static readonly JoinArena JoinArena = new();
    private static readonly Raid Raid = new();
    private static readonly UnlockRuneSlot UnlockRuneSlot = new();

    private static readonly BattleType[] BattleTypes = Enum.GetValues<BattleType>();

    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (await TryProcessRuneSlotStateAsync(actionPlainValue, actionType, session, stoppingToken))
        {
            return;
        }

        if (await TryProcessUnlockRuneSlotAsync(actionPlainValue, actionType, session, stoppingToken))
        {
            return;
        }

        throw new InvalidOperationException(
            $"{nameof(RuneSlotStateHandler)} not support the action type: {actionType}");
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        IValue actionPlainValue,
        string actionType,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
        {
            BattleArena.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(BattleArena, session, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^event_dungeon_battle[0-9]*$"))
        {
            EventDungeonBattle.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(EventDungeonBattle, session, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash[0-9]*$"))
        {
            HackAndSlash.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(HackAndSlash, session, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash_sweep[0-9]*$"))
        {
            HackAndSlash.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(HackAndSlashSweep, session, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            HackAndSlash.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(JoinArena, session, stoppingToken);
        }

        if (Regex.IsMatch(actionType, "^raid[0-9]*$"))
        {
            HackAndSlash.LoadPlainValue(actionPlainValue);
            return await TryProcessRuneSlotStateAsync(Raid, session, stoppingToken);
        }

        return false;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        BattleArena action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.myAvatarAddress,
            action.runeInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        EventDungeonBattle action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.AvatarAddress,
            action.RuneInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        HackAndSlash action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.AvatarAddress,
            action.RuneInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        HackAndSlashSweep action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.avatarAddress,
            action.runeInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        JoinArena action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.runeInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.avatarAddress,
            action.runeInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        Raid action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (action.RuneInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            action.AvatarAddress,
            action.RuneInfos,
            session,
            stoppingToken);
        return true;
    }

    private async Task<bool> TryProcessUnlockRuneSlotAsync(
        IValue actionPlainValue,
        string actionType,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (Regex.IsMatch(actionType, "^unlock_rune_slot[0-9]*$"))
        {
            BattleArena.LoadPlainValue(actionPlainValue);
            return await TryProcessUnlockRuneSlotAsync(UnlockRuneSlot, session, stoppingToken);
        }

        return false;
    }

    private async Task<bool> TryProcessUnlockRuneSlotAsync(
        UnlockRuneSlot action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        foreach (var battleType in BattleTypes)
        {
            await RuneSlotCollectionUpdater.UpdateAsync(
                StateService,
                Store,
                battleType,
                action.AvatarAddress,
                action.SlotIndex,
                session,
                stoppingToken);
        }

        return true;
    }
}
