using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IBalanceRepository
{
    Task<BalanceDocument> GetByAddressAsync(Currency currency, Address address);
}

public class BalanceRepository(IMongoDbService dbService) : IBalanceRepository
{
    public async Task<BalanceDocument> GetByAddressAsync(Currency currency, Address address)
    {
        var accountAddress = new Address(currency.Hash.ToByteArray());
        var collectionName = CollectionNames.GetCollectionName(accountAddress);
        var collection = dbService.GetCollection<BalanceDocument>(collectionName);
        var filter = Builders<BalanceDocument>.Filter.Eq("_id", address.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'");
        }

        return document;
    }
}
