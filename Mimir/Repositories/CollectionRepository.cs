using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models.Assets;
using Mimir.MongoDB.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class CollectionRepository(MongoDbService dbService)
{
    public Collection GetCollection(Address avatarAddress)
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.Collection);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
        }

        try
        {
            var doc = document["Object"].AsBsonDocument;
            return new Collection(doc);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"].AsBsonDocument",
                e);
        }
    }
}
