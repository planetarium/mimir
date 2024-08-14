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

public class JoinArenaHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^join_arena[0-9]*$",
        Log.ForContext<JoinArenaHandler>()
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
        if (action is not IJoinArenaV1 joinArena)
        {
            return false;
        }

        Logger.Information("Handle join_arena, address: {AvatarAddress}", joinArena.AvatarAddress);

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            joinArena.AvatarAddress,
            joinArena.Costumes,
            joinArena.Equipments,
            session,
            stoppingToken
        );

        await ProcessArena(joinArena, session, stoppingToken);
        await ProcessAvatar(joinArena, session, stoppingToken);

        return true;
    }

    private async Task ProcessArena(
        IJoinArenaV1 joinArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var arenaScore = await StateGetter.GetArenaScoreAsync(
            joinArena.AvatarAddress,
            joinArena.ChampionshipId,
            joinArena.Round,
            stoppingToken
        );
        var arenaInfo = await StateGetter.GetArenaInformationAsync(
            joinArena.AvatarAddress,
            joinArena.ChampionshipId,
            joinArena.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            arenaScore,
            arenaInfo,
            joinArena.AvatarAddress,
            joinArena.ChampionshipId,
            joinArena.Round,
            session,
            stoppingToken
        );
    }

    private async Task ProcessAvatar(
        IJoinArenaV1 joinArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var avatarState = await StateGetter.GetAvatarState(joinArena.AvatarAddress, stoppingToken);

        await AvatarCollectionUpdater.UpsertAsync(Store, avatarState, session, stoppingToken);
    }
}
