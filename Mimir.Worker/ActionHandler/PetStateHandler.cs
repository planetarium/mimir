using System.Globalization;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Serilog;
using CombinationSlotState = Mimir.Worker.Models.CombinationSlotState;
using NCCombinationSlotState = Nekoyume.Model.State.CombinationSlotState;

namespace Mimir.Worker.ActionHandler;

public class PetStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^pet_enhancement[0-9]*$|^combination_equipment[0-9]*$|^rapid_combination[0-9]*$",
        Log.ForContext<CombinationSlotStateHandler>()
    )
{
    protected override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        IValue? actionPlainValueInternal
    )
    {
        {
            if (actionPlainValueInternal is not Dictionary actionValues)
            {
                throw new InvalidTypeOfActionPlainValueInternalException(
                    [ValueKind.Dictionary],
                    actionPlainValueInternal?.Kind
                );
            }

            Address avatarAddress;
            int petId;

            if (
                System.Text.RegularExpressions.Regex.IsMatch(
                    actionType,
                    "^pet_enhancement[0-9]*$"
                )
            )
            {
                avatarAddress = new Address(actionValues["a"]);
                petId = (Integer)actionValues["p"];
            }
            else if (
                System.Text.RegularExpressions.Regex.IsMatch(actionType, "^combination_equipment[0-9]*$")
            )
            {
                avatarAddress = new Address(actionValues["a"]);
                petId = (Integer)actionValues["pid"];
            }
            else if (
                System.Text.RegularExpressions.Regex.IsMatch(
                    actionType,
                    "^rapid_combination[0-9]*$"
                )
            )
            {
                avatarAddress = new Address(actionValues["avatarAddress"]);
                int slotIndex = (Integer)actionValues["slotIndex"];

                var slotAddress = avatarAddress.Derive(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        NCCombinationSlotState.DeriveFormat,
                        slotIndex
                    )
                );

                var combinationSlotState = await StateGetter.GetCombinationSlotState(slotAddress);

                if (combinationSlotState is null)
                {
                    Logger.Error(
                        "CombinationSlotState is null\navatar: {avatarAddress}, slotIndex: {slotIndex}",
                        avatarAddress,
                        slotIndex
                    );

                    return;
                }

                if (combinationSlotState.PetId is null)
                {
                    return;
                }
                
                
                petId = combinationSlotState.PetId.Value;
            }
            else
            {
                throw new ArgumentException($"Unknown actionType: {actionType}");
            }

            Logger.Information(
                "{HandlerName}, avatar: {avatarAddress}",
                nameof(PetStateHandler),
                avatarAddress
            );

            var petStateAddress = PetState.DeriveAddress(avatarAddress, petId);
            if (!(await StateGetter.GetPetState(petStateAddress) is { } petState))
            {
                Logger.Error(
                    "PetState is null. (address: {address})",
                    petStateAddress
                );

                return;
            }

            await Store.UpsertStateDataAsync(
                new StateData(
                    petStateAddress,
                    petState
                )
            );
        }
    }
}
