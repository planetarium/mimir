using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Driver;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.Worker.CollectionUpdaters;

public static class ArenaCollectionUpdater
{
    public static async Task UpdateArenaCollectionAsync(
        StateGetter stateGetter,
        MongoDbService dbService,
        Address avatarAddress,
        RoundData roundData,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var arenaInfo = await stateGetter.GetArenaInformationAsync(
            avatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken);
        var arenaScore = await stateGetter.GetArenaScoreAsync(
            avatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken);
        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ArenaDocument>(),
            [
                new ArenaDocument(
                    avatarAddress,
                    roundData,
                    arenaInfo,
                    arenaScore),
            ],
            session,
            stoppingToken
        );
    }
}
