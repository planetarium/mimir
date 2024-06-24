using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.Handler;

public class BattleArenaHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^battle_arena[0-9]*$",
        Log.ForContext<BattleArenaHandler>())
{
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IAction action)
    {
        if (action is not IBattleArenaV1 battleArena)
        {
            throw new NotImplementedException(
                $"Action is not {nameof(IBattleArenaV1)}: {action.GetType()}");
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            battleArena.MyAvatarAddress,
            battleArena.Costumes,
            battleArena.Equipments);

        await UpdateArenaCollectionAsync(
            blockIndex,
            battleArena.MyAvatarAddress,
            battleArena.EnemyAvatarAddress);
    }

    private async Task UpdateArenaCollectionAsync(
        long processBlockIndex,
        Address myAvatarAddress,
        Address enemyAvatarAddress)
    {
        Logger.Information(
            "Handle battle_arena, my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}",
            myAvatarAddress,
            enemyAvatarAddress
        );

        try
        {
            var roundData = await StateGetter.GetArenaRoundData(processBlockIndex);
            var myArenaScore = await StateGetter.GetArenaScoreState(
                myAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var myArenaInfo = await StateGetter.GetArenaInfoState(
                myAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var enemyArenaScore = await StateGetter.GetArenaScoreState(
                enemyAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var enemyArenaInfo = await StateGetter.GetArenaInfoState(
                enemyAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );

            await Store.UpsertStateDataAsyncWithLinkAvatar(
                new StateData(
                    myArenaScore.Address,
                    new ArenaState(
                        myArenaScore,
                        myArenaInfo,
                        roundData,
                        myArenaScore.Address,
                        myAvatarAddress
                    )
                ),
                myAvatarAddress
            );
            await Store.UpsertStateDataAsyncWithLinkAvatar(
                new StateData(
                    enemyArenaScore.Address,
                    new ArenaState(
                        enemyArenaScore,
                        enemyArenaInfo,
                        roundData,
                        enemyArenaScore.Address,
                        enemyAvatarAddress
                    )
                ),
                enemyAvatarAddress
            );
        }
        catch (InvalidCastException ex)
        {
            Logger.Error(
                "Failed to arena states. my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}, error:\n{Error}",
                myAvatarAddress,
                enemyAvatarAddress,
                ex.Message
            );
        }
    }
}
