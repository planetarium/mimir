using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ArenaStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^battle_arena[0-9]*$",
        Log.ForContext<ArenaStateHandler>())
{
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
        {
            var action = new BattleArena();
            action.LoadPlainValue(actionPlainValue);
            await ProcessBattleArena(action, session, stoppingToken);   
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            var action = new JoinArena();
            action.LoadPlainValue(actionPlainValue);
            await ProcessJoinArena(action, session, stoppingToken);   
        }
    }

    private async Task ProcessBattleArena(
        BattleArena battleArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var myArenaScore = await StateGetter.GetArenaScoreAsync(
            battleArena.myAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            stoppingToken);
        var myArenaInfo = await StateGetter.GetArenaInformationAsync(
            battleArena.myAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            stoppingToken);
        var myAvatarState = await StateGetter.GetAvatarState(
            battleArena.myAvatarAddress,
            stoppingToken);
        var mySimpleAvatarState = SimplifiedAvatarState.FromAvatarState(myAvatarState);
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            mySimpleAvatarState,
            myArenaScore,
            myArenaInfo,
            battleArena.myAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            session,
            stoppingToken);

        var enemyArenaScore = await StateGetter.GetArenaScoreAsync(
            battleArena.enemyAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            stoppingToken);
        var enemyArenaInfo = await StateGetter.GetArenaInformationAsync(
            battleArena.enemyAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            stoppingToken);
        var enemyAvatarState = await StateGetter.GetAvatarState(
            battleArena.enemyAvatarAddress,
            stoppingToken);
        var enemySimpleAvatarState = SimplifiedAvatarState.FromAvatarState(enemyAvatarState);
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            enemySimpleAvatarState,
            enemyArenaScore,
            enemyArenaInfo,
            battleArena.enemyAvatarAddress,
            battleArena.championshipId,
            battleArena.round,
            session,
            stoppingToken);
    }
    
    private async Task ProcessJoinArena(
        JoinArena joinArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var arenaScore = await StateGetter.GetArenaScoreAsync(
            joinArena.avatarAddress,
            joinArena.championshipId,
            joinArena.round,
            stoppingToken);
        var arenaInfo = await StateGetter.GetArenaInformationAsync(
            joinArena.avatarAddress,
            joinArena.championshipId,
            joinArena.round,
            stoppingToken);
        var avatarState = await StateGetter.GetAvatarState(
            joinArena.avatarAddress,
            stoppingToken);
        var simpleAvatarState = SimplifiedAvatarState.FromAvatarState(avatarState);
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            simpleAvatarState,
            arenaScore,
            arenaInfo,
            joinArena.avatarAddress,
            joinArena.championshipId,
            joinArena.round,
            session,
            stoppingToken);
    }
}
