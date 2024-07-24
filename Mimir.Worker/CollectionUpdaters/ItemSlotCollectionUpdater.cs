using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;

namespace Mimir.Worker.CollectionUpdaters;

public static class ItemSlotCollectionUpdater
{
    public static async Task UpdateAsync(
        IStateService stateService,
        MongoDbService store,
        BattleType battleType,
        Address avatarAddress,
        IEnumerable<Guid> costumes,
        IEnumerable<Guid> equipments,
        IClientSessionHandle? session = null
    )
    {
        var collectionName = CollectionNames.GetCollectionName<ItemSlotState>();
        var collection = store.GetCollection(collectionName);
        var itemSlotAddress = Nekoyume.Model.State.ItemSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        var orderedCostumes = costumes.OrderBy(e => e).ToList();
        var orderedEquipments = equipments.OrderBy(e => e).ToList();
        if (
            !HasChanged(collection, itemSlotAddress, orderedCostumes, orderedEquipments)
            || await stateService.GetState(itemSlotAddress) is not List serialized
        )
        {
            return;
        }

        var itemSlotState = new Nekoyume.Model.State.ItemSlotState(serialized);
        var stateData = new StateData(itemSlotAddress, new ItemSlotState(itemSlotState));

        await store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ItemSlotState>(),
            [stateData],
            session
        );
    }

    private static bool HasChanged(
        IMongoCollection<BsonDocument> collection,
        Address itemSlotAddress,
        List<Guid> costumes,
        List<Guid> equipments
    )
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", itemSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return true;
        }

        try
        {
            var storedCostumes = document["State"]
                ["Object"]["Costumes"]
                .AsBsonArray.Select(e => Guid.Parse(e.AsString))
                .OrderBy(e => e)
                .ToArray();
            if (!storedCostumes.SequenceEqual(costumes))
            {
                return true;
            }

            var storedEquipments = document["State"]
                ["Object"]["Equipments"]
                .AsBsonArray.Select(e => Guid.Parse(e.AsString))
                .OrderBy(e => e)
                .ToArray();
            return !storedEquipments.SequenceEqual(equipments);
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
    }
}
