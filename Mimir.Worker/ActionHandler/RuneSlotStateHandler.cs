using Bencodex.Types;
using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RuneSlotStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^(battle_arena[0-9]*|event_dungeon_battle[0-9]*|join_arena[0-9]*|hack_and_slash[0-9]*|hack_and_slash_sweep[0-9]*|raid[0-9]*|unlock_rune_slot[0-9]*)$",
        Log.ForContext<RuneSlotStateHandler>()
    )
{
    private static readonly BattleType[] BattleTypes = Enum.GetValues<BattleType>();

    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        Logger.Information(
            "RuneSlotStateHandler, address: {Signer}",
            signer
        );

        if (await TryProcessRuneSlotStateAsync(action, session, stoppingToken))
        {
            return true;
        }

        if (await TryProcessUnlockRuneSlotAsync(action, session, stoppingToken))
        {
            return true;
        }

        return false;
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        (BattleType battleType, Address avatarAddress, IEnumerable<IValue>? runeSlotInfos)? tuple =
            action switch
            {
                IBattleArenaV1 ba => (BattleType.Arena, ba.MyAvatarAddress, ba.RuneSlotInfos),
                IEventDungeonBattleV2 edb
                    => (BattleType.Adventure, edb.AvatarAddress, edb.RuneSlotInfos),
                IJoinArenaV1 ja => (BattleType.Arena, ja.AvatarAddress, ja.RuneSlotInfos),
                IHackAndSlashV10 has
                    => (BattleType.Adventure, has.AvatarAddress, has.RuneSlotInfos),
                IHackAndSlashSweepV3 hass
                    => (BattleType.Adventure, hass.AvatarAddress, hass.RuneSlotInfos),
                IRaidV2 r => (BattleType.Raid, r.AvatarAddress, r.RuneSlotInfos),
                _ => null,
            };
        if (tuple is null)
        {
            return false;
        }

        var (battleType, avatarAddress, runeSlotInfos) = tuple.Value;
        if (runeSlotInfos is null)
        {
            // ignore
            return false;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            battleType,
            avatarAddress,
            runeSlotInfos,
            session,
            stoppingToken
        );
        return true;
    }

    private async Task<bool> TryProcessUnlockRuneSlotAsync(
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IUnlockRuneSlotV1 unlockRuneSlot)
        {
            return false;
        }

        foreach (var battleType in BattleTypes)
        {
            await RuneSlotCollectionUpdater.UpdateAsync(
                StateService,
                Store,
                battleType,
                unlockRuneSlot.AvatarAddress,
                unlockRuneSlot.SlotIndex,
                session,
                stoppingToken
            );
        }

        return true;
    }
}
