using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class PledgeCollectionUpdater
{
    public static WriteModel<BsonDocument> UpsertAsync(
        long blockIndex,
        Address address,
        Address contractAddress,
        bool contracted,
        int refillMead
    )
    {
        return new PledgeDocument(
            blockIndex,
            address,
            contractAddress,
            contracted,
            refillMead
        ).ToUpdateOneModel();
    }

    public static UpdateOneModel<BsonDocument> ApproveAsync(Address address)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", address.ToString());
        var update = Builders<BsonDocument>.Update.Set("Contracted", true);
        return new UpdateOneModel<BsonDocument>(filter, update);
    }

    public static DeleteOneModel<BsonDocument> DeleteAsync(Address address)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", address.ToString());
        return new DeleteOneModel<BsonDocument>(filter);
    }
}
