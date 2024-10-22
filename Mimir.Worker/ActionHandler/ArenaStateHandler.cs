using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using LiteDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;
using BsonDocument = MongoDB.Bson.BsonDocument;

namespace Mimir.Worker.ActionHandler;

public class ArenaStateHandler(IStateService stateService, MongoDbService store, IHeadlessGQLClient headlessGqlClient, IInitializerManager initializerManager)
    : BaseActionHandler<ArenaDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^battle_arena[0-9]*$|^join_arena[0-9]*$",
        Log.ForContext<ArenaStateHandler>())
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
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
            return await ProcessBattleArena(action, session, stoppingToken);   
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            var action = new JoinArena();
            action.LoadPlainValue(actionPlainValue);
            return await ProcessJoinArena(action, session, stoppingToken);   
        }

        return [];
    }

    private async Task<IEnumerable<WriteModel<BsonDocument>>> ProcessBattleArena(
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
        return
        [
            ArenaCollectionUpdater.UpsertAsync(
                mySimpleAvatarState,
                myArenaScore,
                myArenaInfo,
                battleArena.myAvatarAddress,
                battleArena.championshipId,
                battleArena.round),
            ArenaCollectionUpdater.UpsertAsync(
                enemySimpleAvatarState,
                enemyArenaScore,
                enemyArenaInfo,
                battleArena.enemyAvatarAddress,
                battleArena.championshipId,
                battleArena.round),
        ];
    }
    
    private async Task<IEnumerable<WriteModel<BsonDocument>>> ProcessJoinArena(
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
        return
        [
            ArenaCollectionUpdater.UpsertAsync(
                simpleAvatarState,
                arenaScore,
                arenaInfo,
                joinArena.avatarAddress,
                joinArena.championshipId,
                joinArena.round),
        ];
    }
}
