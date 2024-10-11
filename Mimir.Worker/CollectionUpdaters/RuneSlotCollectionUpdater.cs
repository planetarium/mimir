using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using RuneSlotState = Lib9c.Models.States.RuneSlotState;

namespace Mimir.Worker.CollectionUpdaters;

public static class RuneSlotCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        BattleType battleType,
        Address avatarAddress,
        IEnumerable<RuneSlotInfo> runeSlotInfos,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var collectionName = CollectionNames.GetCollectionName<RuneSlotDocument>();
        var collection = store.GetCollection(collectionName);
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType);
        var orderedRuneSlotInfos = runeSlotInfos
            .OrderBy(e => e.SlotIndex)
            .ToList();
        if (!HasChanged(collection, runeSlotAddress, orderedRuneSlotInfos) ||
            await stateService.GetState(runeSlotAddress, stoppingToken) is not List serialized)
        {
            return;
        }

        var runeSlotState = new RuneSlotState(serialized);
        var runeSlotDocument = new RuneSlotDocument(runeSlotAddress, runeSlotState);
        await store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<RuneSlotDocument>(),
            [runeSlotDocument],
            session,
            stoppingToken);
    }

    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        BattleType battleType,
        Address avatarAddress,
        int runeSlotIndexToUnlock,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var collectionName = CollectionNames.GetCollectionName<RuneSlotDocument>();
        var collection = store.GetCollection(collectionName);
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType);
        if (!HasChanged(collection, runeSlotAddress, runeSlotIndexToUnlock) ||
            await stateService.GetState(runeSlotAddress, stoppingToken) is not List serialized)
        {
            return;
        }

        var runeSlotState = new RuneSlotState(serialized);
        var runeSlotDocument = new RuneSlotDocument(runeSlotAddress, runeSlotState);
        await store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<RuneSlotDocument>(),
            [runeSlotDocument],
            session,
            stoppingToken);
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
            var storedRuneSlots = document["Object"]
                ["Slots"]
                .AsBsonArray.OfType<BsonDocument>()
                .Select(e =>
                    (
                        slotIndex: e["Index"].AsInt32,
                        runeId: e.TryGetValue("RuneSheetId", out var runeSheetIdBsonValue)
                            ? runeSheetIdBsonValue.AsNullableInt32
                            : null
                    )
                )
                .OrderBy(tuple => tuple.slotIndex)
                .ToArray();
            if (storedRuneSlots.Length != runeSlotInfos.Count)
            {
                return true;
            }

            for (var i = 0; i < storedRuneSlots.Length; i++)
            {
                if (storedRuneSlots[i].slotIndex != runeSlotInfos[i].SlotIndex ||
                    storedRuneSlots[i].runeId != runeSlotInfos[i].RuneId)
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
            return document["Object"]["Slots"].AsBsonArray
                .Select(e =>
                    (
                        slotIndex: e["SlotIndex"].AsInt32,
                        isLock: e["IsLock"].AsBoolean
                    )
                )
                .Any(tuple => tuple.slotIndex == runeSlotIndexToUnlock &&
                              tuple.isLock);
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
    }
}
