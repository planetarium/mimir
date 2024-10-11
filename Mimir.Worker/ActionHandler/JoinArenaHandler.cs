using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class JoinArenaHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^join_arena[0-9]*$",
        Log.ForContext<JoinArenaHandler>())
{
    private static readonly JoinArena Action = new();

    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        Action.LoadPlainValue(actionPlainValue);
        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            Action.avatarAddress,
            Action.costumes,
            Action.equipments,
            session,
            stoppingToken);
        await ProcessArena(Action, session, stoppingToken);
    }

    private async Task ProcessArena(
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
