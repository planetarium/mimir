using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;

namespace Mimir.Repositories;

public class RuneSlotRepository(MongoDbService dbService)
{
    public async Task<RuneSlotDocument> GetByAddressAsync(Address avatarAddress, BattleType battleType)
    {
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType);
        var collectionName = CollectionNames.GetCollectionName<RuneSlotDocument>();
        var collection = dbService.GetCollection<RuneSlotDocument>(collectionName);
        var filter = Builders<RuneSlotDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{runeSlotAddress.ToHex()}'");
        }

        return document;
    }
}
