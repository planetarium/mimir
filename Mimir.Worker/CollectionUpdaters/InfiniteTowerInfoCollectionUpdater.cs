using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume;
using Nekoyume.Action;

namespace Mimir.Worker.CollectionUpdaters;

public static class InfiniteTowerInfoCollectionUpdater
{
    public static async Task<IEnumerable<WriteModel<BsonDocument>>> UpdateAsync(
        IStateService stateService,
        long blockIndex,
        Address avatarAddress,
        int infiniteTowerId,
        CancellationToken stoppingToken = default
    )
    {
        var accountAddress = Addresses.InfiniteTowerInfo.Derive($"{infiniteTowerId}");
        var state = await stateService.GetState(avatarAddress, accountAddress, stoppingToken);

        if (state is not List serialized)
        {
            return [];
        }

        var infiniteTowerInfo = new InfiniteTowerInfo(serialized);
        var infiniteTowerInfoDocument = new InfiniteTowerInfoDocument(
            blockIndex,
            accountAddress,
            avatarAddress,
            infiniteTowerId,
            infiniteTowerInfo
        );
        return [infiniteTowerInfoDocument.ToUpdateOneModel()];
    }
}
