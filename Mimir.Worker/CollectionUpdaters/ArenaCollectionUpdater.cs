using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class ArenaCollectionUpdater
{
    public static WriteModel<BsonDocument> UpsertAsync(
        SimplifiedAvatarState simpleAvatar,
        ArenaScore arenaScore,
        ArenaInformation arenaInfo,
        Address avatarAddress,
        int championshipId,
        int round
    )
    {
        return new ArenaDocument(avatarAddress, championshipId, round, arenaInfo, arenaScore, simpleAvatar)
            .ToUpdateOneModel();
    }
}
