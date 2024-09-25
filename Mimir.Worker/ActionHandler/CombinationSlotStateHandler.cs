using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class CombinationSlotStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^combination_consumable[0-9]*$|^combination_equipment[0-9]*$|^event_consumable_item_crafts[0-9]*$|^item_enhancement[0-9]*$|^rapid_combination[0-9]*$",
        Log.ForContext<CombinationSlotStateHandler>())
{
    protected override async Task<bool> TryHandleAction(
        string actionType,
        long processBlockIndex,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            var e = new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind
            );
            Logger.Fatal(
                e,
                "Unexpected actionPlainValueInternal type: {ActionPlainValueInternalType}",
                actionPlainValueInternal?.Kind);
            return false;
        }

        Address avatarAddress;
        int slotIndex;

        if (Regex.IsMatch(actionType, "^combination_consumable[0-9]*$") ||
            Regex.IsMatch(actionType, "^combination_equipment[0-9]*$"))
        {
            avatarAddress = new Address(actionValues["a"]);
            slotIndex = actionValues["s"].ToInteger();
        }
        else if (Regex.IsMatch(actionType, "^item_enhancement[0-9]*$"))
        {
            avatarAddress = new Address(actionValues["avatarAddress"]);
            slotIndex = actionValues["slotIndex"].ToInteger();
        }
        else if (Regex.IsMatch(actionType, "^event_consumable_item_crafts[0-9]*$"))
        {
            if (actionValues["l"] is not List list)
            {
                var e = new ArgumentException("'l' must be a bencodex list");
                Logger.Fatal(e, "actionValues[\"l\"] is not a List");
                return false;
            }

            avatarAddress = new Address(list[0]);
            slotIndex = list[3].ToInteger();
        }
        else if (Regex.IsMatch(actionType, "^rapid_combination[0-9]*$"))
        {
            avatarAddress = new Address(actionValues["avatarAddress"]);
            slotIndex = actionValues["slotIndex"].ToInteger();
        }
        else
        {
            var e = new ArgumentException($"Unknown actionType: {actionType}");
            Logger.Fatal(e, "Unknown actionType: {ActionType}", actionType);
            return false;
        }

        Logger.Information("CombinationSlotStateHandler, avatar: {AvatarAddress}", avatarAddress);

        var slotAddress = Nekoyume.Model.State.CombinationSlotState.DeriveAddress(avatarAddress, slotIndex);
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
            avatarAddress,
            slotIndex,
            combinationSlotState);
        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<CombinationSlotStateDocument>(),
            [doc],
            session,
            stoppingToken
        );

        return true;
    }
}
