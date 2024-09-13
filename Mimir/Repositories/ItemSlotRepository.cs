using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Repositories;

public class ItemSlotRepository(MongoDbService dbService)
{
    public ItemSlotState GetItemSlot(
        Address avatarAddress,
        BattleType battleType)
    {
        var itemSlotAddress = ItemSlotState.DeriveAddress(avatarAddress, battleType);
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.ItemSlot);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", itemSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{itemSlotAddress.ToHex()}'");
        }

        try
        {
            var obj = document["Object"].AsBsonDocument;
            var costumes = obj["Costumes"].AsBsonArray
                .Select(e => Guid.Parse(e.AsString))
                .ToList();
            var equipments = obj["Equipments"].AsBsonArray
                .Select(e => Guid.Parse(e.AsString))
                .ToList();
            var itemSlot = new ItemSlotState(battleType);
            itemSlot.UpdateCostumes(costumes);
            itemSlot.UpdateEquipment(equipments);
            return itemSlot;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"] or its children keys",
                e);
        }
        catch (InvalidCastException e)
        {
            throw new UnexpectedTypeOfBsonValueException(
                "document[\"State\"][\"Object\"].AsBsonDocument or its children values",
                e);
        }
    }
}
