using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.GraphQL.Extensions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.Stake;

namespace Mimir.Repositories;

public class StakeRepository(MongoDbService dbService)
{
    public StakeStateV2 GetStakeState(Address agentAddress)
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.Stake.Value);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", agentAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{agentAddress.ToHex()}'");
        }

        try
        {
            var doc = document["State"]["Object"].AsBsonDocument;
            var contractDoc = doc["Contract"].AsBsonDocument;
            var contract = new Contract(
                contractDoc["StakeRegularFixedRewardSheetTableName"].AsString,
                contractDoc["StakeRegularRewardSheetTableName"].AsString,
                contractDoc["RewardInterval"].ToLong(),
                contractDoc["LockupInterval"].ToLong());
            var startedBlockIndex = doc["StartedBlockIndex"].ToLong();
            var receivedBlockIndex = doc["ReceivedBlockIndex"].ToLong();
            return new StakeStateV2(contract, startedBlockIndex, receivedBlockIndex);
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
