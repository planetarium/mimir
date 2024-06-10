using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

public class BattleArenaHandler : BaseActionHandler
{
    public BattleArenaHandler(IStateService stateService, MongoDbService store)
        : base(stateService, store, "^battle_arena[0-9]*$") { }

    protected override async Task HandleAction(long processBlockIndex, Dictionary actionValues)
    {
        var myAvatarAddress = new Address(actionValues["maa"]);
        var enemyAvatarAddress = new Address(actionValues["eaa"]);

        var roundData = await _stateGetter.GetArenaRoundData(processBlockIndex);
        var myArenaScore = await _stateGetter.GetArenaScore(roundData, myAvatarAddress);
        var myArenaInfo = await _stateGetter.GetArenaInfo(roundData, myAvatarAddress);
        var enemyArenaScore = await _stateGetter.GetArenaScore(roundData, enemyAvatarAddress);
        var enemyArenaInfo = await _stateGetter.GetArenaInfo(roundData, enemyAvatarAddress);

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
}
