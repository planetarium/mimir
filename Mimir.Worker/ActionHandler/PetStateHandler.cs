using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class PetStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler<PetStateDocument>(
        stateService,
        store,
        "^pet_enhancement[0-9]*$|^combination_equipment[0-9]*$|^rapid_combination[0-9]*$",
        Log.ForContext<PetStateHandler>())
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind);
        }

        Address avatarAddress;
        int petId;

        if (Regex.IsMatch(actionType, "^pet_enhancement[0-9]*$"))
        {
            avatarAddress = actionValues["a"].ToAddress();
            petId = actionValues["p"].ToInteger();
        }
        else if (Regex.IsMatch(actionType, "^combination_equipment[0-9]*$"))
        {
            avatarAddress = actionValues["a"].ToAddress();
            var pid = actionValues["pid"].ToNullableInteger();
            if (pid is null)
            {
                return [];
            }

            petId = pid.Value;
        }
        else if (Regex.IsMatch(actionType, "^rapid_combination[0-9]*$"))
        {
            avatarAddress = actionValues["avatarAddress"].ToAddress();
            var slotIndex = actionValues["slotIndex"].ToInteger();
            var allCombinationSlotState = await StateGetter.GetAllCombinationSlotStateAsync(
                avatarAddress,
                stoppingToken);

            if (!allCombinationSlotState.CombinationSlots.TryGetValue(slotIndex, out var combinationSlotState))
            {
                throw new InvalidOperationException($"CombinationSlotState not found for slotIndex: {slotIndex}");
            }

            if (combinationSlotState.PetId is null)
            {
                // ignore
                return [];
            }

            petId = combinationSlotState.PetId.Value;
        }
        else
        {
            throw new ArgumentException($"Unknown actionType: {actionType}");
        }

        var petStateAddress = Nekoyume.Model.State.PetState.DeriveAddress(avatarAddress, petId);
        var petState = await StateGetter.GetPetState(petStateAddress, stoppingToken);
        return [new PetStateDocument(petStateAddress, avatarAddress, petState).ToUpdateOneModel()];
    }
}
