using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ArenaRepository(MongoDbService dbService)
{
    public async Task<long> GetRankingByAvatarAddressAsync(
        Address avatarAddress,
        int championshipId,
        int round)
    {
        var collection = dbService.GetCollection<ArenaDocument>(CollectionNames.Arena.Value);
        return await GetRankingByAvatarAddressAsync(
            collection,
            avatarAddress,
            championshipId,
            round);
    }

    private static async Task<long> GetRankingByAvatarAddressAsync(
        IMongoCollection<ArenaDocument> collection,
        Address avatarAddress,
        int championshipId,
        int round)
    {
        var pipelines = new BsonDocument[]
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("ChampionshipId", championshipId),
                        new BsonDocument("Round", round)
                    }
                )
            ),
            new("$sort", new BsonDocument("ArenaScore.Score", -1)),
            new(
                "$group",
                new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "docs", new BsonDocument("$push", "$$ROOT") }
                }
            ),
            new(
                "$unwind",
                new BsonDocument { { "path", "$docs" }, { "includeArrayIndex", "Rank" } }
            ),
            new("$match", new BsonDocument("docs.Address", avatarAddress.ToHex()))
        };

        var aggregation = await collection.Aggregate<dynamic>(pipelines).ToListAsync();
        return aggregation.Count == 0 ? 0 : (long)aggregation.First().Rank + 1;
    }

    public async Task<List<ArenaRankingDocument>> GetLeaderboardAsync(
        int championshipId,
        int round,
        long skip,
        int limit)
    {
        var collection = dbService.GetCollection<ArenaDocument>(CollectionNames.Arena.Value);
        return await GetLeaderboardAsync(collection, championshipId, round, skip, limit);
    }

    private static async Task<List<ArenaRankingDocument>> GetLeaderboardAsync(
        IMongoCollection<ArenaDocument> collection,
        int championshipId,
        int round,
        long skip,
        int limit)
    {
        var pipelines = new List<BsonDocument>
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("ChampionshipId", championshipId),
                        new BsonDocument("Round", round)
                    }
                )
            ),
            new("$sort", new BsonDocument("ArenaScore.Score", -1)),
            new(
                "$group",
                new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "docs", new BsonDocument("$push", "$$ROOT") }
                }
            ),
            new(
                "$unwind",
                new BsonDocument
                {
                    { "path", "$docs" },
                    { "includeArrayIndex", "docs.Rank" }
                }
            ),
            new("$replaceRoot", new BsonDocument("newRoot", "$docs")),
            new("$skip", skip),
            new("$limit", limit),
            new(
                "$lookup",
                new BsonDocument
                {
                    { "from", "avatar" },
                    { "localField", "Address" },
                    { "foreignField", "Address" },
                    { "as", "SimpleAvatar" }
                }
            ),
            new(
                "$unwind",
                new BsonDocument
                {
                    { "path", "$SimpleAvatar" },
                    { "preserveNullAndEmptyArrays", true }
                }
            ),
            new(
                "$project",
                new BsonDocument
                {
                    { "SimpleAvatar.Object.MailBox", 0 },
                    { "SimpleAvatar.Object.StageMap", 0 },
                    { "SimpleAvatar.Object.MonsterMap", 0 },
                    { "SimpleAvatar.Object.ItemMap", 0 },
                    { "SimpleAvatar.Object.EventMap", 0 }
                }
            ),
            new(
                "$addFields",
                new BsonDocument("SimpleAvatar", "$SimpleAvatar.Object")
            ),
            new("$sort", new BsonDocument("Rank", 1))
        };

        var aggregation = await collection.Aggregate<ArenaRankingDocument>(pipelines).ToListAsync();
        return UpdateRank(aggregation.Where(e => e is not null).ToList());
    }

    private static List<ArenaRankingDocument> UpdateRank(List<ArenaRankingDocument> source)
    {
        source = source
            .Select(e =>
            {
                e.Rank++;
                return e;
            })
            .ToList();
        int? currentScore = null;
        var currentRank = 1;
        var trunk = new List<ArenaRankingDocument>();
        var result = new List<ArenaRankingDocument>();
        for (var i = 0; i < source.Count; i++)
        {
            var arenaRanking = source[i];
            if (!currentScore.HasValue)
            {
                currentScore = arenaRanking.ArenaScore.Score;
                trunk.Add(arenaRanking);
                continue;
            }

            if (currentScore.Value == arenaRanking.ArenaScore.Score)
            {
                trunk.Add(arenaRanking);
                currentRank++;
                if (i < source.Count - 1)
                {
                    continue;
                }

                TrunkToResult();
                continue;
            }

            TrunkToResult();
            if (i < source.Count - 1)
            {
                trunk.Add(arenaRanking);
                currentScore = arenaRanking.ArenaScore.Score;
                currentRank++;
                continue;
            }

            arenaRanking.Rank = currentRank + 1;
            result.Add(arenaRanking);
        }

        return result;

        void TrunkToResult()
        {
            if (trunk.Count == 1)
            {
                result.Add(trunk[0]);
            }
            else
            {
                foreach (var inTrunk in trunk.OrderBy(e => e.Address))
                {
                    inTrunk.Rank = currentRank;
                    result.Add(inTrunk);
                }
            }

            trunk.Clear();
        }
    }
}
