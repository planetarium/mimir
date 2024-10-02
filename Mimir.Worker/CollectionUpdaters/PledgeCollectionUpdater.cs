using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class PledgeCollectionUpdater
{
    public static async Task UpsertAsync(
        MongoDbService dbService,
        Address address,
        Address contractAddress,
        bool contracted,
        int refillMead,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ArenaDocument>(),
            [new PledgeDocument(address, contractAddress, contracted, refillMead),],
            session,
            stoppingToken
        );
    }

    public static async Task ApproveAsync(
        MongoDbService dbService,
        Address address,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var collection = dbService.GetCollection<PledgeDocument>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToString());
        var update = Builders<BsonDocument>.Update.Set("Contracted", true);
        await (session is null
            ? collection.UpdateOneAsync(session, filter, update, null, stoppingToken)
            : collection.UpdateOneAsync(filter, update, null, stoppingToken));
    }

    public static async Task DeleteAsync(
        MongoDbService dbService,
        Address address,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var collection = dbService.GetCollection<PledgeDocument>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToString());
        await (session is null
            ? collection.DeleteOneAsync(session, filter, null, stoppingToken)
            : collection.DeleteOneAsync(filter, null, stoppingToken));
    }
}
