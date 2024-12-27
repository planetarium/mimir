using Bencodex.Types;
using Libplanet.Crypto;
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
    public static async Task<IEnumerable<WriteModel<BsonDocument>>> UpdateAsync(
        long blockIndex,
        IStateService stateService,
        BattleType battleType,
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        if (await stateService.GetState(runeSlotAddress, stoppingToken) is not List serialized)
        {
            return [];
        }

        var runeSlotState = new RuneSlotState(serialized);
        var runeSlotDocument = new RuneSlotDocument(
            blockIndex,
            avatarAddress,
            runeSlotAddress,
            runeSlotState
        );
        return [runeSlotDocument.ToUpdateOneModel()];
    }

    private static bool HasChanged(
        IMongoCollection<BsonDocument> collection,
        Address runeSlotAddress,
        List<RuneSlotInfo> runeSlotInfos
    )
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
                if (
                    storedRuneSlots[i].slotIndex != runeSlotInfos[i].SlotIndex
                    || storedRuneSlots[i].runeId != runeSlotInfos[i].RuneId
                )
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
        int runeSlotIndexToUnlock
    )
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return true;
        }

        try
        {
            return document["Object"]
                ["Slots"]
                .AsBsonArray.Select(e =>
                    (slotIndex: e["SlotIndex"].AsInt32, isLock: e["IsLock"].AsBoolean)
                )
                .Any(tuple => tuple.slotIndex == runeSlotIndexToUnlock && tuple.isLock);
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
    }
}
