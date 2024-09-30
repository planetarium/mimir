using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.Exceptions;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RapidCombinationHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^rapid_combination[0-9]*$",
        Log.ForContext<RapidCombinationHandler>())
{
    protected override async Task<bool> TryHandleAction(
        string actionType,
        long processBlockIndex,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (actionPlainValueInternal is not Dictionary d)
        {
            var e = new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind);
            Logger.Fatal(
                e,
                "Unexpected actionPlainValueInternal type: {ActionPlainValueInternalType}",
                actionPlainValueInternal?.Kind);
            return false;
        }

        var avatarAddress = d.TryGetValue((Text)"a", out var avatarAddressValue)
            ? new Address(avatarAddressValue)
            : (Address?)null;
        if (avatarAddress is null)
        {
            var e = new InvalidOperationException(
                "Avatar address is missing in the actionPlainValueInternal dictionary. \"a\"");
            Logger.Fatal(e, "Failed to deserialization");
            return false;
        }

        var slotIndexes = d.TryGetValue((Text)"s", out var slotIndexesValue)
            ? ((List)slotIndexesValue).Select(e => (int)(Integer)e)
            : null;
        if (slotIndexes is null)
        {
            var e = new InvalidOperationException(
                "Slot indexes are missing in the actionPlainValueInternal dictionary. \"s\"");
            Logger.Fatal(e, "Failed to deserialization");
            return false;
        }

        foreach (var slotIndex in slotIndexes)
        {
            var slotAddress = Nekoyume.Model.State.CombinationSlotState.DeriveAddress(avatarAddress.Value, slotIndex);
            CombinationSlotState combinationSlotState;
            try
            {
                combinationSlotState = await StateGetter.GetCombinationSlotStateAsync(slotAddress, stoppingToken);
            }
            catch (StateNotFoundException e)
            {
                Logger.Fatal(
                    e,
                    "CombinationSlotState is null\navatar: {AvatarAddress}, slotIndex: {SlotIndex}",
                    avatarAddress,
                    slotIndex);
                return false;
            }

            var doc = new CombinationSlotStateDocument(
                slotAddress,
                avatarAddress.Value,
                slotIndex,
                combinationSlotState);
            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<CombinationSlotStateDocument>(),
                [doc],
                session,
                stoppingToken);
        }

        return true;
    }
}
