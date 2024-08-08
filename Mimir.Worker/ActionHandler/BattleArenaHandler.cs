using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Constants;
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

        await UpdateArenaCollectionAsync(
            blockIndex,
            battleArena.MyAvatarAddress,
            battleArena.EnemyAvatarAddress,
            session,
            stoppingToken
        );

        return true;
    }

    private async Task UpdateArenaCollectionAsync(
        long processBlockIndex,
        Address myAvatarAddress,
        Address enemyAvatarAddress,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        Logger.Information(
            "Handle battle_arena, my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}",
            myAvatarAddress,
            enemyAvatarAddress
        );

        var roundData = await StateGetter.GetArenaRoundData(processBlockIndex, stoppingToken);
        var myArenaScore = await StateGetter.GetArenaScoreState(
            myAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var myArenaInfo = await StateGetter.GetArenaInfoState(
            myAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var enemyArenaScore = await StateGetter.GetArenaScoreState(
            enemyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var enemyArenaInfo = await StateGetter.GetArenaInfoState(
            enemyAvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );

        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ArenaDocument>(),
            [
                new ArenaDocument(
                    myArenaScore.Address,
                    myArenaInfo.Address,
                    myArenaInfo,
                    myArenaScore,
                    roundData,
                    myAvatarAddress
                ),
                new ArenaDocument(
                    enemyArenaScore.Address,
                    enemyArenaInfo.Address,
                    enemyArenaInfo,
                    enemyArenaScore,
                    roundData,
                    enemyAvatarAddress
                ),
            ],
            session,
            stoppingToken
        );
    }
}
