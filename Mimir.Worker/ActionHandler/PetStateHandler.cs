using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class PetStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration
)
    : BaseActionHandler<PetStateDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^pet_enhancement[0-9]*$|^combination_equipment[0-9]*$|^rapid_combination[0-9]*$",
        Log.ForContext<PetStateHandler>(),
        configuration
    )
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind
            );
        }

        Address avatarAddress;
        var petIds = new List<int>();
        if (Regex.IsMatch(actionType, "^pet_enhancement[0-9]*$"))
        {
            avatarAddress = actionValues["a"].ToAddress();
            petIds.Add(actionValues["p"].ToInteger());
        }
        else if (Regex.IsMatch(actionType, "^combination_equipment[0-9]*$"))
        {
            avatarAddress = actionValues["a"].ToAddress();
            var pid = actionValues["pid"].ToNullableInteger();
            if (pid is null)
            {
                return [];
            }

            petIds.Add(pid.Value);
        }
        else if (Regex.IsMatch(actionType, "^rapid_combination[0-9]*$"))
        {
            avatarAddress = actionValues.TryGetValue((Text)"a", out var avatarAddressValue)
                ? avatarAddressValue.ToAddress()
                : actionValues["avatarAddress"].ToAddress();
            var slotIndexes = actionValues.TryGetValue((Text)"s", out var slotIndexValue)
                ? slotIndexValue.ToList(i => (int)(Integer)i)
                : [actionValues["slotIndex"].ToInteger()];
            var allCombinationSlotState = await StateGetter.GetAllCombinationSlotStateAsync(
                avatarAddress,
                stoppingToken
            );
            foreach (var slotIndex in slotIndexes)
            {
                if (
                    !allCombinationSlotState.CombinationSlots.TryGetValue(
                        slotIndex,
                        out var combinationSlotState
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"CombinationSlotState not found for slotIndex: {slotIndex}"
                    );
                }

                if (combinationSlotState.PetId is null)
                {
                    // ignore
                    continue;
                }

                petIds.Add(combinationSlotState.PetId.Value);
            }
        }
        else
        {
            throw new ArgumentException($"Unknown actionType: {actionType}");
        }

        var petStateAddresses = petIds
            .Select(e => Nekoyume.Model.State.PetState.DeriveAddress(avatarAddress, e))
            .ToArray();
        var petStates = (
            await StateGetter.GetPetStates(petStateAddresses, stoppingToken)
        ).ToArray();
        return petStateAddresses
            .Select(
                (e, i) =>
                    new PetStateDocument(
                        blockIndex,
                        e,
                        avatarAddress,
                        petStates[i]
                    ).ToUpdateOneModel()
            )
            .ToArray();
    }
}
