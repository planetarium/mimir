using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Models.Abstractions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Repositories;

public class RuneSlotRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public IRuneSlots GetRuneSlots(
        PlanetName planetName,
        Address avatarAddress,
        BattleType battleType)
    {
        var runeSlotAddress = RuneSlotState.DeriveAddress(avatarAddress, battleType);
        var collection = GetCollection(planetName);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{runeSlotAddress.ToHex()}'");
        }

        try
        {
            var slots = document["State"]["Object"]["Slots"].AsBsonArray
                .OfType<BsonDocument>()
                .Select(doc =>
                {
                    var slotIndex = doc["SlotIndex"].AsInt32;
                    var runeSlotType = (RuneSlotType)doc["RuneSlotType"].AsInt32;
                    var runeType = (RuneType)doc["RuneType"].AsInt32;
                    var isLock = doc["IsLock"].AsBoolean;
                    var runeSheetId = doc.Contains("RuneSheetId")
                        ? doc["RuneSheetId"].AsNullableInt32
                        : null;
                    return new RuneSlot(
                        slotIndex,
                        runeSlotType,
                        runeType,
                        isLock,
                        runeSheetId);
                });
            return new RuneSlots(runeSlotAddress, battleType, slots);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"][\"slots\"] or its children keys",
                e);
        }
        catch (InvalidCastException e)
        {
            throw new UnexpectedTypeOfBsonValueException(
                "document[\"State\"][\"Object\"][\"slots\"].AsBsonArray or its children values",
                e);
        }
    }

    protected override string GetCollectionName() => "rune_slot";
}
