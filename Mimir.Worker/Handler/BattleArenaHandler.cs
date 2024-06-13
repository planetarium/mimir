using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Handler;

public class BattleArenaHandler : BaseActionHandler
{
    public BattleArenaHandler(IStateService stateService, MongoDbService store)
        : base(stateService, store, "^battle_arena[0-9]*$", Log.ForContext<BattleArenaHandler>())
    { }

    public override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        Dictionary actionValues
    )
    {
        var myAvatarAddress = new Address(actionValues["maa"]);
        var enemyAvatarAddress = new Address(actionValues["eaa"]);

        _logger.Information(
            "Handle battle_arena, my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}",
            myAvatarAddress,
            enemyAvatarAddress
        );

        try
        {
            var roundData = await _stateGetter.GetArenaRoundData(processBlockIndex);
            var myArenaScore = await _stateGetter.GetArenaScoreState(
                myAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var myArenaInfo = await _stateGetter.GetArenaInfoState(
                myAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var enemyArenaScore = await _stateGetter.GetArenaScoreState(
                enemyAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );
            var enemyArenaInfo = await _stateGetter.GetArenaInfoState(
                enemyAvatarAddress,
                roundData.ChampionshipId,
                roundData.Round
            );

            await _store.UpsertStateDataAsyncWithLinkAvatar(
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
            await _store.UpsertStateDataAsyncWithLinkAvatar(
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
        catch (InvalidOperationException ex)
        {
            _logger.Error(
                "Failed to arena states. my: {MyAvatarAddress}, enemy: {EnemyAvatarAddress}, error:\n{Error}",
                myAvatarAddress,
                enemyAvatarAddress,
                ex.Message
            );
        }
    }
}
