using Bencodex.Types;
using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.Handler;

public class RuneSlotStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^(battle_arena[0-9]*|event_dungeon_battle[0-9]*|join_arena[0-9]*|hack_and_slash[0-9]*|hack_and_slash_sweep[0-9]*|raid[0-9]*|unlock_rune_slot[0-9]*)$",
        Log.ForContext<RuneSlotStateHandler>())
{
    private static readonly BattleType[] BattleTypes = Enum.GetValues<BattleType>();

    protected override async Task HandleAction(long blockIndex, IAction action)
    {
        if (await TryProcessRuneSlotStateAsync(action))
        {
            return;
        }

        if (await TryProcessUnlockRuneSlotAsync(action))
        {
            return;
        }

        throw new NotImplementedException();
    }

    private async Task<bool> TryProcessRuneSlotStateAsync(IAction action)
    {
        (BattleType battleType, Address avatarAddress, IEnumerable<IValue>? runeSlotInfos)? tuple = action switch
        {
            IBattleArenaV1 ba => (BattleType.Arena, ba.MyAvatarAddress, ba.RuneSlotInfos),
            IEventDungeonBattleV2 edb => (BattleType.Adventure, edb.AvatarAddress, edb.RuneSlotInfos),
            IJoinArenaV1 ja => (BattleType.Arena, ja.AvatarAddress, ja.RuneSlotInfos),
            IHackAndSlashV10 has => (BattleType.Adventure, has.AvatarAddress, has.RuneSlotInfos),
            IHackAndSlashSweepV3 hass => (BattleType.Adventure, hass.AvatarAddress, hass.RuneSlotInfos),
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
            return true;
        }

        await RuneSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            battleType,
            avatarAddress,
            runeSlotInfos);
        return true;
    }

    private async Task<bool> TryProcessUnlockRuneSlotAsync(IAction action)
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
                unlockRuneSlot.SlotIndex);
        }

        return true;
    }
}
