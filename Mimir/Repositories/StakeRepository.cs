using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class StakeRepository(MongoDbService dbService)
{
    // public async Task<StakeDocument> GetByAddressAsync(Address agentAddress)
    // {
    //     var collectionName = CollectionNames.GetCollectionName<StakeDocument>();
    //     var collection = dbService.GetCollection<StakeDocument>(collectionName);
    //     var filter = Builders<StakeDocument>.Filter.Eq("Address", agentAddress.ToHex());
    //     var document = await collection.Find(filter).FirstOrDefaultAsync();
    //     if (document is null)
    //     {
    //         throw new DocumentNotFoundInMongoCollectionException(
    //             collection.CollectionNamespace.CollectionName,
    //             $"'Address' equals to '{agentAddress.ToHex()}'");
    //     }
    //
    //     return document;
    // }
}
