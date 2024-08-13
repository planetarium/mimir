using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Driver;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.Worker.CollectionUpdaters;

public static class ArenaCollectionUpdater
{
    public static async Task UpsertAsync(
        MongoDbService dbService,
        ArenaScore arenaScore,
        ArenaInformation arenaInfo,
        Address avatarAddress,
        RoundData roundData,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ArenaDocument>(),
            [
                new ArenaDocument(
                    avatarAddress,
                    roundData.ChampionshipId,
                    roundData.Round,
                    arenaInfo,
                    arenaScore),
            ],
            session,
            stoppingToken
        );
    }
}
