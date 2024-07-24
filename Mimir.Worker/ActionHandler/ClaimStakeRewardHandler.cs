// using Lib9c.Abstractions;
// using Libplanet.Action;
// using Libplanet.Crypto;
// using Mimir.Worker.CollectionUpdaters;
// using Mimir.Worker.Services;
// using MongoDB.Driver;
// using Serilog;

// namespace Mimir.Worker.ActionHandler;

// public class ClaimStakeRewardHandler(IStateService stateService, MongoDbService store)
//     : BaseActionHandler(
//         stateService,
//         store,
//         "^claim_stake_reward[0-9]*$",
//         Log.ForContext<ClaimStakeRewardHandler>()
//     )
// {
//     protected override async Task<bool> TryHandleAction(
//         IClientSessionHandle session,
//         long blockIndex,
//         Address signer,
//         IAction action
//     )
//     {
//         if (action is not IClaimStakeRewardV1)
//         {
//             return false;
//         }

//         await StakeCollectionUpdater.UpdateAsync(session, StateService, Store, signer);

//         return true;
//     }
// }
