using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class AvatarCollectionUpdater
{
    public static WriteModel<BsonDocument> UpsertAsync(long blockIndex, AvatarState avatarState)
    {
        return new AvatarDocument(blockIndex, avatarState.Address, avatarState).ToUpdateOneModel();
    }
}
