using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class BattleArenaHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^battle_arena[0-9]*$",
        Log.ForContext<BattleArenaHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IBattleArenaV1 battleArena)
        {
            return false;
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            battleArena.MyAvatarAddress,
            battleArena.Costumes,
            battleArena.Equipments,
            null,
            stoppingToken
        );

        Logger.Information(
            "Handle battle_arena, my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}",
            battleArena.MyAvatarAddress,
            battleArena.EnemyAvatarAddress
        );

        var stateGetter = stateService.At();
        var roundData = await stateGetter.GetArenaRoundData(blockIndex, stoppingToken);

        var myArenaScore = await stateGetter.GetArenaScoreState(
            battleArena.MyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var myArenaInfo = await stateGetter.GetArenaInfoState(
            battleArena.MyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            myArenaScore,
            myArenaInfo,
            battleArena.MyAvatarAddress,
            roundData,
            session,
            stoppingToken
        );

        var enemyArenaScore = await stateGetter.GetArenaScoreState(
            battleArena.EnemyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var enemyArenaInfo = await stateGetter.GetArenaInfoState(
            battleArena.EnemyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            enemyArenaScore,
            enemyArenaInfo,
            battleArena.EnemyAvatarAddress,
            roundData,
            session,
            stoppingToken
        );

        return true;
    }
}
