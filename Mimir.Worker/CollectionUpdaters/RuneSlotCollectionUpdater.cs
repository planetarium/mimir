using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;

namespace Mimir.Worker.CollectionUpdaters;

public static class RuneSlotCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        BattleType battleType,
        Address avatarAddress,
        IEnumerable<IValue> runeSlotInfos)
    {
        var collectionName = CollectionNames.GetCollectionName<ItemSlotState>();
        var collection = store.GetCollection(collectionName);
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(avatarAddress, battleType);
        var orderedRuneSlotInfos = runeSlotInfos
            .OfType<List>()
            .Select(e => new RuneSlotInfo(e))
            .OrderBy(e => e.SlotIndex)
            .ToList();
        if (!HasChanged(collection, runeSlotAddress, orderedRuneSlotInfos) ||
            await stateService.GetState(runeSlotAddress) is not List serialized)
        {
            return;
        }

        var runeSlotState = new Nekoyume.Model.State.RuneSlotState(serialized);
        var runeSlots = new RuneSlots(runeSlotAddress, runeSlotState);
        var stateData = new StateData(
            runeSlotAddress,
            new RuneSlotState(runeSlotAddress, runeSlots));
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData, avatarAddress);
    }

    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        BattleType battleType,
        Address avatarAddress,
        int runeSlotIndexToUnlock)
    {
        var collectionName = CollectionNames.GetCollectionName<ItemSlotState>();
        var collection = store.GetCollection(collectionName);
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(avatarAddress, battleType);
        if (!HasChanged(collection, runeSlotAddress, runeSlotIndexToUnlock) ||
            await stateService.GetState(runeSlotAddress) is not List serialized)
        {
            return;
        }

        var runeSlotState = new Nekoyume.Model.State.RuneSlotState(serialized);
        var runeSlots = new RuneSlots(runeSlotAddress, runeSlotState);
        var stateData = new StateData(
            runeSlotAddress,
            new RuneSlotState(runeSlotAddress, runeSlots));
        await store.UpsertStateDataAsyncWithLinkAvatar(stateData, avatarAddress);
    }

    private static bool HasChanged(
        IMongoCollection<BsonDocument> collection,
        Address runeSlotAddress,
        List<RuneSlotInfo> runeSlotInfos)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return true;
        }

        try
        {
            var storedCostumes = document["State"]["Object"]["Slots"].AsBsonArray
                .OfType<BsonDocument>()
                .Select(e => (
                    slotIndex: e["SlotIndex"].AsInt32,
                    runeId: e.Contains("RuneSheetId")
                        ? e["RuneSheetId"].AsNullableInt32
                        : null))
                .OrderBy(tuple => tuple.slotIndex)
                .ToArray();
            if (storedCostumes.Length != runeSlotInfos.Count)
            {
                return true;
            }

            for (var i = 0; i < storedCostumes.Length; i++)
            {
                if (storedCostumes[i].slotIndex != runeSlotInfos[i].SlotIndex ||
                    storedCostumes[i].runeId != runeSlotInfos[i].RuneId)
                {
                    return true;
                }
            }
        }
        catch (KeyNotFoundException)
        {
            return true;
        }

        return false;
    }

    private static bool HasChanged(
        IMongoCollection<BsonDocument> collection,
        Address runeSlotAddress,
        int runeSlotIndexToUnlock)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return true;
        }

        try
        {
            return document["State"]["Object"]["Slots"].AsBsonArray
                .Select(e => (
                    slotIndex: e["SlotIndex"].AsInt32,
                    isLock: e["IsLock"].AsBoolean))
                .Any(tuple => tuple.slotIndex == runeSlotIndexToUnlock && tuple.isLock);
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
    }
}
