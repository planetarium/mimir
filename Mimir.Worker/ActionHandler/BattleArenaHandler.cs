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

        await ProcessArena(blockIndex, battleArena, session, stoppingToken);
        await ProcessAvatar(battleArena, session, stoppingToken);

        return true;
    }

    private async Task ProcessArena(
        long blockIndex,
        IBattleArenaV1 battleArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var myArenaScore = await StateGetter.GetArenaScoreAsync(
            battleArena.MyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            stoppingToken
        );
        var myArenaInfo = await StateGetter.GetArenaInformationAsync(
            battleArena.MyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            myArenaScore,
            myArenaInfo,
            battleArena.MyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            session,
            stoppingToken
        );

        var enemyArenaScore = await StateGetter.GetArenaScoreAsync(
            battleArena.EnemyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            stoppingToken
        );
        var enemyArenaInfo = await StateGetter.GetArenaInformationAsync(
            battleArena.EnemyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            enemyArenaScore,
            enemyArenaInfo,
            battleArena.EnemyAvatarAddress,
            battleArena.ChampionshipId,
            battleArena.Round,
            session,
            stoppingToken
        );
    }

    private async Task ProcessAvatar(
        IBattleArenaV1 battleArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var myAvatarState = await StateGetter.GetAvatarState(
            battleArena.MyAvatarAddress,
            stoppingToken
        );
        var enemyAvatarState = await StateGetter.GetAvatarState(
            battleArena.EnemyAvatarAddress,
            stoppingToken
        );
        await AvatarCollectionUpdater.UpsertAsync(Store, myAvatarState, session, stoppingToken);
        await AvatarCollectionUpdater.UpsertAsync(Store, enemyAvatarState, session, stoppingToken);
    }
}
