using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Rune;
using Nekoyume.Model.State;

namespace Mimir.Repositories;

public class RuneSlotRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public RuneSlot[] GetRuneSlots(
        PlanetName planetName,
        Address avatarAddress,
        BattleType battleType)
    {
        var itemSlotAddress = RuneSlotState.DeriveAddress(avatarAddress, battleType);
        var collection = GetCollection(planetName);
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
            return document["State"]["Object"]["slots"].AsBsonArray
                .Select(doc =>
                {
                    var slotIndex = doc["SlotIndex"].AsInt32;
                    var runeSlotType = (RuneSlotType)doc["RuneSlotType"].AsInt32;
                    var runeType = (RuneType)doc["RuneType"].AsInt32;
                    var isLock = doc["IsLock"].AsBoolean;
                    var runeSheetId = doc["RuneSheetId"].AsNullableInt32;
                    var runeSlot = new RuneSlot(slotIndex, runeSlotType, runeType, isLock);
                    if (runeSheetId.HasValue)
                    {
                        runeSlot.Equip(runeSheetId.Value);
                    }

                    return runeSlot;
                })
                .ToArray();
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"][\"slots\"] or its children keys",
                e);
        }
    }

    protected override string GetCollectionName() => "rune_slot";
}
