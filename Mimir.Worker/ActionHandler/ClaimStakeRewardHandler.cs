using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ClaimStakeRewardHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^claim_stake_reward[0-9]*$",
        Log.ForContext<StakeHandler>())
{
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IAction action)
    {
        if (action is not IClaimStakeRewardV1)
        {
            throw new NotImplementedException(
                $"Action is not {nameof(IClaimStakeRewardV1)}: {action.GetType()}");
        }

        await StakeCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            signer);
    }
}
