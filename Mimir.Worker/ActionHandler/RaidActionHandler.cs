// using Bencodex.Types;
// using Libplanet.Crypto;
// using Mimir.Worker.Constants;
// using Mimir.Worker.Exceptions;
// using Mimir.Worker.Models;
// using Mimir.Worker.Services;
// using MongoDB.Driver;
// using Nekoyume;
// using Nekoyume.Extensions;
// using Nekoyume.Model.State;
// using Nekoyume.TableData;
// using Serilog;
// using RaiderState = Mimir.Worker.Models.RaiderState;
// using WorldBossState = Mimir.Worker.Models.WorldBossState;

// namespace Mimir.Worker.ActionHandler;

// public class RaidActionHandler(IStateService stateService, MongoDbService store)
//     : BaseActionHandler(stateService, store, "^raid[0-9]*$", Log.ForContext<RaidActionHandler>())
// {
//     protected override async Task<bool> TryHandleAction(
//         IClientSessionHandle session,
//         string actionType,
//         long processBlockIndex,
//         IValue? actionPlainValueInternal
//     )
//     {
//         if (actionPlainValueInternal is not Dictionary actionValues)
//         {
//             throw new InvalidTypeOfActionPlainValueInternalException(
//                 [ValueKind.Dictionary],
//                 actionPlainValueInternal?.Kind
//             );
//         }

//         var avatarAddress = actionValues["a"].ToAddress();

//         Logger.Information("Handle raid, avatar: {avatarAddress}", avatarAddress);

//         var worldBossListSheet = await Store.GetSheetAsync<WorldBossListSheet>();

//         if (worldBossListSheet != null)
//         {
//             int raidId;
//             try
//             {
//                 var row = worldBossListSheet.FindRowByBlockIndex(processBlockIndex);
//                 raidId = row.Id;
//             }
//             catch (InvalidOperationException)
//             {
//                 Logger.Error("Failed to get this raidId.");
//                 return false;
//             }

//             var worldBossAddress = Addresses.GetWorldBossAddress(raidId);
//             var raiderAddress = Addresses.GetRaiderAddress(avatarAddress, raidId);
//             var worldBossKillRewardRecordAddress = Addresses.GetWorldBossKillRewardRecordAddress(
//                 avatarAddress,
//                 raidId
//             );

//             var worldBossState = await StateGetter.GetWorldBossState(worldBossAddress);
//             var raiderState = await StateGetter.GetRaiderState(raiderAddress);
//             var worldBossKillRewardRecordState =
//                 await StateGetter.GetWorldBossKillRewardRecordState(
//                     worldBossKillRewardRecordAddress
//                 );

//             await Store.UpsertStateDataManyAsync(
//                 session,
//                 CollectionNames.GetCollectionName<WorldBossState>(),
//                 [new StateData(worldBossAddress, new WorldBossState(raidId, worldBossState))]
//             );

//             await Store.UpsertStateDataManyAsync(
//                 session,
//                 CollectionNames.GetCollectionName<RaiderState>(),
//                 [new StateData(raiderAddress, new RaiderState(raiderState))]
//             );

//             await Store.UpsertStateDataManyAsync(
//                 session,
//                 CollectionNames.GetCollectionName<WorldBossKillRewardRecordState>(),
//                 [
//                     new StateData(
//                         worldBossKillRewardRecordAddress,
//                         new WorldBossKillRewardRecordState(
//                             avatarAddress,
//                             worldBossKillRewardRecordState
//                         )
//                     )
//                 ]
//             );
//         }
//         else
//         {
//             Logger.Error("RaidActionHandler requires worldBossListSheet.");
//         }

//         return true;
//     }
// }
