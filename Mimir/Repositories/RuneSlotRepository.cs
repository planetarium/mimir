using Libplanet.Crypto;
using Lib9c.Models.Runes;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using RuneSlotState = Lib9c.Models.States.RuneSlotState;

namespace Mimir.Repositories;

public class RuneSlotRepository(MongoDbService dbService)
{
    public RuneSlotState GetRuneSlotState(
        Address avatarAddress,
        BattleType battleType)
    {
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(avatarAddress, battleType);
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.RuneSlot.Value);
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
            var slots = document["Object"]["Slots"].AsBsonArray
                .OfType<BsonDocument>()
                .Select(doc =>
                {
                    var slotIndex = doc["Index"].AsInt32;
                    var runeSlotType = (RuneSlotType)doc["RuneSlotType"].AsInt32;
                    var runeType = (RuneType)doc["RuneType"].AsInt32;
                    var isLock = doc["IsLock"].AsBoolean;
                    var runeSheetId = doc.Contains("RuneId")
                        ? doc["RuneId"].AsNullableInt32
                        : null;
                    return new RuneSlot(
                        slotIndex,
                        runeSlotType,
                        runeType,
                        isLock,
                        runeSheetId);
                })
                .ToList();
            return new RuneSlotState(battleType, slots);
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
}
