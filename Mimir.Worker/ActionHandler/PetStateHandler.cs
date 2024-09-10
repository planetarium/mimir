using System.Globalization;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class PetStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^pet_enhancement[0-9]*$|^combination_equipment[0-9]*$|^rapid_combination[0-9]*$",
        Log.ForContext<PetStateHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        string actionType,
        long processBlockIndex,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
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

            if (System.Text.RegularExpressions.Regex.IsMatch(actionType, "^pet_enhancement[0-9]*$"))
            {
                avatarAddress = actionValues["a"].ToAddress();
                petId = actionValues["p"].ToInteger();
            }
            else if (
                System.Text.RegularExpressions.Regex.IsMatch(
                    actionType,
                    "^combination_equipment[0-9]*$"
                )
            )
            {
                avatarAddress = actionValues["a"].ToAddress();
                var pid = actionValues["pid"].ToNullableInteger();
                if (pid is null)
                {
                    return false;
                }
                petId = pid.Value;
            }
            else if (
                System.Text.RegularExpressions.Regex.IsMatch(
                    actionType,
                    "^rapid_combination[0-9]*$"
                )
            )
            {
                avatarAddress = actionValues["avatarAddress"].ToAddress();
                int slotIndex = actionValues["slotIndex"].ToInteger();

                var slotAddress = avatarAddress.Derive(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Nekoyume.Model.State.CombinationSlotState.DeriveFormat,
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

                    return false;
                }

                if (combinationSlotState.PetId is null)
                {
                    return false;
                }

                petId = combinationSlotState.PetId.Value;
            }
            else
            {
                throw new ArgumentException($"Unknown actionType: {actionType}");
            }

            Logger.Information("Handle pet_state, avatar: {AvatarAddress} ", avatarAddress);

            var petStateAddress = Nekoyume.Model.State.PetState.DeriveAddress(avatarAddress, petId);
            var petState = await StateGetter.GetPetState(petStateAddress);

            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<PetStateDocument>(),
                [new PetStateDocument(petStateAddress, petState)],
                session,
                stoppingToken
            );

            return true;
        }
    }
}
