// using System.Globalization;
// using Bencodex.Types;
// using Libplanet.Crypto;
// using Mimir.Worker.Constants;
// using Mimir.Worker.Exceptions;
// using Mimir.Worker.Models;
// using Mimir.Worker.Services;
// using MongoDB.Driver;
// using Nekoyume.Action;
// using Nekoyume.Model.State;
// using Serilog;
// using NCCombinationSlotState = Nekoyume.Model.State.CombinationSlotState;
// using NCPetState = Nekoyume.Model.State.PetState;
// using PetState = Mimir.Worker.Models.PetState;

// namespace Mimir.Worker.ActionHandler;

// public class PetStateHandler(IStateService stateService, MongoDbService store)
//     : BaseActionHandler(
//         stateService,
//         store,
//         "^pet_enhancement[0-9]*$|^combination_equipment[0-9]*$|^rapid_combination[0-9]*$",
//         Log.ForContext<CombinationSlotStateHandler>()
//     )
// {
//     protected override async Task<bool> TryHandleAction(
//         IClientSessionHandle session,
//         string actionType,
//         long processBlockIndex,
//         IValue? actionPlainValueInternal
//     )
//     {
//         {
//             if (actionPlainValueInternal is not Dictionary actionValues)
//             {
//                 throw new InvalidTypeOfActionPlainValueInternalException(
//                     [ValueKind.Dictionary],
//                     actionPlainValueInternal?.Kind
//                 );
//             }

//             Address avatarAddress;
//             int petId;

//             if (System.Text.RegularExpressions.Regex.IsMatch(actionType, "^pet_enhancement[0-9]*$"))
//             {
//                 avatarAddress = actionValues["a"].ToAddress();
//                 petId = actionValues["p"].ToInteger();
//             }
//             else if (
//                 System.Text.RegularExpressions.Regex.IsMatch(
//                     actionType,
//                     "^combination_equipment[0-9]*$"
//                 )
//             )
//             {
//                 avatarAddress = actionValues["a"].ToAddress();
//                 var pid = actionValues["pid"].ToNullableInteger();
//                 if (pid is null)
//                 {
//                     return false;
//                 }
//                 petId = pid.Value;
//             }
//             else if (
//                 System.Text.RegularExpressions.Regex.IsMatch(
//                     actionType,
//                     "^rapid_combination[0-9]*$"
//                 )
//             )
//             {
//                 avatarAddress = actionValues["avatarAddress"].ToAddress();
//                 int slotIndex = actionValues["slotIndex"].ToInteger();

//                 var slotAddress = avatarAddress.Derive(
//                     string.Format(
//                         CultureInfo.InvariantCulture,
//                         NCCombinationSlotState.DeriveFormat,
//                         slotIndex
//                     )
//                 );

//                 var combinationSlotState = await StateGetter.GetCombinationSlotState(slotAddress);

//                 if (combinationSlotState is null)
//                 {
//                     Logger.Error(
//                         "CombinationSlotState is null\navatar: {avatarAddress}, slotIndex: {slotIndex}",
//                         avatarAddress,
//                         slotIndex
//                     );

//                     return false;
//                 }

//                 if (combinationSlotState.PetId is null)
//                 {
//                     return false;
//                 }

//                 petId = combinationSlotState.PetId.Value;
//             }
//             else
//             {
//                 throw new ArgumentException($"Unknown actionType: {actionType}");
//             }

//             Logger.Information(
//                 "{HandlerName}, avatar: {avatarAddress}",
//                 nameof(PetStateHandler),
//                 avatarAddress
//             );

//             var petStateAddress = NCPetState.DeriveAddress(avatarAddress, petId);
//             if (!(await StateGetter.GetPetState(petStateAddress) is { } petState))
//             {
//                 Logger.Error("PetState is null. (address: {address})", petStateAddress);

//                 return false;
//             }

//             await Store.UpsertStateDataManyAsync(
//                 session,
//                 CollectionNames.GetCollectionName<PetState>(),
//                 [new StateData(petStateAddress, new PetState(petState))]
//             );

//             return true;
//         }
//     }
// }
