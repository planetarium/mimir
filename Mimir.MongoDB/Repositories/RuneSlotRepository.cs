using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;

namespace Mimir.MongoDB.Repositories;

public interface IRuneSlotRepository
{
    Task<RuneSlotDocument> GetByAddressAsync(Address avatarAddress, BattleType battleType);
}

public class RuneSlotRepository(MongoDbService dbService) : IRuneSlotRepository
{
    public async Task<RuneSlotDocument> GetByAddressAsync(
        Address avatarAddress,
        BattleType battleType
    )
    {
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        var collectionName = CollectionNames.GetCollectionName<RuneSlotDocument>();
        var collection = dbService.GetCollection<RuneSlotDocument>(collectionName);
        var filter = Builders<RuneSlotDocument>.Filter.Eq("Address", runeSlotAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{runeSlotAddress.ToHex()}'"
            );
        }

        return document;
    }
}
