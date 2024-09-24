using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class ArenaCollectionUpdater
{
    public static async Task UpsertAsync(
        MongoDbService dbService,
        SimplifiedAvatarState simpleAvatar,
        ArenaScore arenaScore,
        ArenaInformation arenaInfo,
        Address avatarAddress,
        int championshipId,
        int round,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ArenaDocument>(),
            [new ArenaDocument(avatarAddress, championshipId, round, arenaInfo, arenaScore, simpleAvatar),],
            session,
            stoppingToken
        );
    }
}
