using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Services;

namespace NineChroniclesUtilBackend.Repositories;

public class ArenaRankingRepository(MongoDBCollectionService mongoDBCollectionService)
{
    private readonly IMongoCollection<dynamic> ArenaCollection = mongoDBCollectionService.GetCollection<dynamic>("arena_0_10");

    public async Task<List<dynamic>> GetRanking(int limit, int offset)
    {
        var pipelines = new BsonDocument[]
        {
            new() { { "$skip", offset } },
            new() { { "$limit", limit } },
            new() {
                {
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "avatars" },
                        { "localField", "AvatarAddress" },
                        { "foreignField", "address" },
                        { "as", "avatar" }
                    }
                }
            },
            new() { { "$unwind", "$avatar" } }
        };

        var aggregation = await ArenaCollection.Aggregate<dynamic>(pipelines).ToListAsync();
        var result = aggregation.Select(x => new
        {
            x.avatar.agentAddress,
            x.AvatarAddress,
            x.ArenaScore.Score,
            AvatarName = x.avatar.name,
        }).ToList<dynamic>();

        return result;
    }
}