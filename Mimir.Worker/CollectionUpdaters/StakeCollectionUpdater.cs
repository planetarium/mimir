using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.Stake;

namespace Mimir.Worker.CollectionUpdaters;

public static class StakeCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        Address agentAddress)
    {
        var stakeAddress = StakeStateV2.DeriveAddress(agentAddress);
        if (await stateService.GetState(stakeAddress) is not { } serialized)
        {
            return;
        }

        var slotState = new StakeStateV2(serialized);
        var stateData = new StateData(
            stakeAddress,
            new StakeState(stakeAddress, slotState));
        await store.UpsertStateDataAsyncWithLinkAgent(stateData, agentAddress);
    }
}
