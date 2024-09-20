using Bencodex.Types;
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
        var stakeState = await stateService.GetState(stakeAddress, stoppingToken);
        StakeDocument document;
        if (stakeState is null ||
            stakeState.Kind == ValueKind.Null)
        {
            document = new StakeDocument(stakeAddress, agentAddress, null);
        }
        else
        {
            document = new StakeDocument(stakeAddress, agentAddress, new StakeState(stakeState));
        }

        try
        {
            await store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<StakeDocument>(),
                [document],
                session,
                stoppingToken
            );
        }
        catch (ArgumentException)
        {
            // Skip stake state v1
            return;
        }
    }
}
