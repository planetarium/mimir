using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class StakeCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        Address agentAddress,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var stakeAddress = Nekoyume.Model.Stake.StakeStateV2.DeriveAddress(agentAddress);
        if (await stateService.GetState(stakeAddress) is not { } bencoded)
        {
            return;
        }

        try
        {
            var stakeState = new StakeState(bencoded);
            var document = new StakeDocument(stakeAddress, agentAddress, stakeState);

            await store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<StakeDocument>(),
                [document],
                session,
                stoppingToken
            );
        }
        catch (System.ArgumentException)
        {
            // Skip stake state v1
            return;
        }
    }
}
