using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Models.Agent;
using NineChroniclesUtilBackend.Models.Arena;
using NineChroniclesUtilBackend.Services;

namespace NineChroniclesUtilBackend.Repositories;

public class ArenaRankingRepository(MongoDBCollectionService mongoDBCollectionService)
{
    private readonly IMongoCollection<dynamic> ArenaCollection = mongoDBCollectionService.GetCollection<dynamic>("arena_0_10");

    public async Task<long> GetRankByAvatarAddress(string avatarAddress)
    {
        var pipelines = new BsonDocument[]
        {
            new("$sort", new BsonDocument("ArenaScore.Score", -1)),
            new("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "docs", new BsonDocument("$push", "$$ROOT") },
            }),
            new("$unwind", new BsonDocument
            {
                { "path", "$docs" },
                { "includeArrayIndex", "Rank" },
            }),
            new("$match", new BsonDocument("docs.AvatarAddress", avatarAddress)),
            new("$replaceRoot", new BsonDocument
            {
                { "newRoot", new BsonDocument
                {
                    { "$mergeObjects", new BsonArray
                    {
                        "$docs",
                        new BsonDocument("Rank", "$Rank"),
                    } },
                } },
            }),
        };

        var aggregation = await ArenaCollection.Aggregate<dynamic>(pipelines).ToListAsync();

        return aggregation.First().Rank + 1;
    }

    public async Task<List<ArenaRanking>> GetRanking(long limit, long offset)
    {
        // TODO: Implement CP Calculation
        var pipelines = new BsonDocument[]
        {
            new("$sort", new BsonDocument("ArenaScore.Score", -1)),
            new("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "docs", new BsonDocument("$push", "$$ROOT") },
            }),
            new("$unwind", new BsonDocument
            {
                { "path", "$docs" },
                { "includeArrayIndex", "Rank" },
            }),
            new("$skip", offset),
            new("$limit", limit),
            new("$replaceRoot", new BsonDocument
            {
                { "newRoot", new BsonDocument
                {
                    { "$mergeObjects", new BsonArray
                    {
                        "$docs",
                        new BsonDocument("Rank", "$Rank"),
                    } },
                } },
            }),
            new("$lookup", new BsonDocument
            {
                { "from", "avatars" },
                { "localField", "AvatarAddress" },
                { "foreignField", "address" },
                { "as", "Avatar" },
            }),
            new("$unwind", new BsonDocument
            {
                { "path", "$Avatar" },
            }),
        };


        var aggregation = await ArenaCollection.Aggregate<dynamic>(pipelines).ToListAsync();
        var result = aggregation.Select(x => new ArenaRanking(
            x.AvatarAddress,
            x.ArenaInfo.Address,
            x.ArenaInfo.Win,
            x.ArenaInfo.Lose,
            x.Rank + 1,
            x.ArenaInfo.Ticket,
            x.ArenaInfo.TicketResetCount,
            x.ArenaInfo.PurchasedTicketCount,
            x.ArenaScore.Score,
            new Avatar(
                x.Avatar.address,
                x.Avatar.name,
                x.Avatar.level
            )
        )).ToList();

        return result;
    }
}