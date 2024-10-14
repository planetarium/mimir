using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class PetRepository(MongoDbService dbService)
{
    public async Task<PetStateDocument> GetByAvatarAddressAsync(Address avatarAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<PetStateDocument>();
        var collection = dbService.GetCollection<PetStateDocument>(collectionName);
        var filter = Builders<PetStateDocument>.Filter.Eq("AvatarAddress", avatarAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'AvatarAddress' equals to '{avatarAddress.ToHex()}'");
        }

        return document;
    }
}
