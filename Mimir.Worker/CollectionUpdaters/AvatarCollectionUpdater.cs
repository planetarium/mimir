using Lib9c.Models.States;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class AvatarCollectionUpdater
{
    public static async Task UpsertAsync(
        MongoDbService dbService,
        AvatarState avatarState,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<AvatarDocument>(),
            [new AvatarDocument(avatarState.Address, avatarState)],
            session,
            stoppingToken
        );
    }
}
