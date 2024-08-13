using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
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
        await ProcessArena(stateGetter, blockIndex, battleArena, session, stoppingToken);
        await ProcessAvatar(stateGetter, battleArena, session, stoppingToken);

        return true;
    }

    private async Task ProcessArena(
        StateGetter stateGetter,
        long blockIndex,
        IBattleArenaV1 battleArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var roundData = await stateGetter.GetArenaRoundData(blockIndex, stoppingToken);

        var myArenaScore = await stateGetter.GetArenaScoreAsync(
            battleArena.MyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var myArenaInfo = await stateGetter.GetArenaInformationAsync(
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
            roundData.ChampionshipId,
            roundData.Round,
            session,
            stoppingToken
        );

        var enemyArenaScore = await stateGetter.GetArenaScoreAsync(
            battleArena.EnemyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var enemyArenaInfo = await stateGetter.GetArenaInformationAsync(
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
            roundData.ChampionshipId,
            roundData.Round,
            session,
            stoppingToken
        );
    }

    private async Task ProcessAvatar(
        StateGetter stateGetter,
        IBattleArenaV1 battleArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var myAvatarState = await stateGetter.GetAvatarState(
            battleArena.MyAvatarAddress,
            stoppingToken
        );
        var enemyAvatarState = await stateGetter.GetAvatarState(
            battleArena.EnemyAvatarAddress,
            stoppingToken
        );
        await AvatarCollectionUpdater.UpsertAsync(Store, myAvatarState, session, stoppingToken);
        await AvatarCollectionUpdater.UpsertAsync(Store, enemyAvatarState, session, stoppingToken);
    }
}
