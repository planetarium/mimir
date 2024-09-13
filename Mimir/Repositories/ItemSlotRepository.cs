using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;

namespace Mimir.Repositories;

public class ItemSlotRepository(MongoDbService dbService)
{
    public async Task<ItemSlotDocument> GetByAddressAsync(Address avatarAddress, BattleType battleType)
    {
        var itemSlotAddress = Nekoyume.Model.State.ItemSlotState.DeriveAddress(
            avatarAddress,
            battleType);
        var collectionName = CollectionNames.GetCollectionName<ItemSlotDocument>();
        var collection = dbService.GetCollection<ItemSlotDocument>(collectionName);
        var filter = Builders<ItemSlotDocument>.Filter.Eq("Address", itemSlotAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{itemSlotAddress.ToHex()}'");
        }

        return document;
    }
}
