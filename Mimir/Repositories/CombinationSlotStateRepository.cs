using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class CombinationSlotStateRepository(MongoDbService dbService)
{
    public async Task<CombinationSlotStateDocument> GetByAvatarAddressAsync(
        Address avatarAddress,
        int slotIndex)
    {
        var address = Nekoyume.Model.State.CombinationSlotState.DeriveAddress(avatarAddress, slotIndex);
        var collectionName = CollectionNames.GetCollectionName<CombinationSlotStateDocument>();
        var collection = dbService.GetCollection<CombinationSlotStateDocument>(collectionName);
        var filter = Builders<CombinationSlotStateDocument>.Filter.Eq("Address", address.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'");
        }

        return document;
    }

    public async Task<List<CombinationSlotStateDocument>> GetByAvatarAddressAsync(Address avatarAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<CombinationSlotStateDocument>();
        var collection = dbService.GetCollection<CombinationSlotStateDocument>(collectionName);
        var builder = Builders<CombinationSlotStateDocument>.Filter;
        FilterDefinition<CombinationSlotStateDocument>? filter = null;
        var addresses = Enumerable.Range(0, Nekoyume.Model.State.AvatarState.CombinationSlotCapacity)
            .Select(slotIndex => Nekoyume.Model.State.CombinationSlotState.DeriveAddress(avatarAddress, slotIndex))
            .ToArray();
        foreach (var address in addresses)
        {
            if (filter is null)
            {
                filter = builder.Eq("Address", address.ToHex());
                continue;
            }

            filter = builder.Or(filter, builder.Eq("Address", address.ToHex()));
        }

        var document = await collection.Find(filter).ToListAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{string.Join(", ", addresses.Select(e => e.ToHex()))}'");
        }

        return document;
    }
}
