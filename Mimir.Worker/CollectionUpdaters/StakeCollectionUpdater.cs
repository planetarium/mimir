using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.Constants;
using MongoDB.Driver;
using Nekoyume.Model.Stake;

namespace Mimir.Worker.CollectionUpdaters;

public static class StakeCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        Address agentAddress,
        IClientSessionHandle? session
    )
    {
        var stakeAddress = StakeStateV2.DeriveAddress(agentAddress);
        if (await stateService.GetState(stakeAddress) is not { } serialized)
        {
            return;
        }

        try
        {
            var slotState = new StakeStateV2(serialized);
            var stateData = new MongoDbCollectionDocument(
                stakeAddress,
                new StakeDocument(stakeAddress, slotState));
            await store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<StakeDocument>(),
                [stateData],
                session
            );
        }
        catch (System.ArgumentException)
        {
            // Skip stake state v1
            return;
        }
    }
}
