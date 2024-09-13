using Libplanet.Crypto;
using Microsoft.Extensions.Caching.Memory;
using Mimir.Enums;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Arena;
using Nekoyume.Model.Arena;
using Nekoyume.Model.EnumType;

namespace Mimir.Repositories;

public class ArenaRepository(MongoDbService dbService)
{
    private static readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetPriority(CacheItemPriority.High)
        .SetSlidingExpiration(TimeSpan.FromMinutes(1));

    private readonly MemoryCache _leaderboardCache = new(new MemoryCacheOptions());
    private readonly SemaphoreSlim _leaderboardCacheLock = new(1, 1);

    private async Task<List<ArenaRankingDocument>> GetOrCreateLeaderboardAsync(
        long blockIndex,
        int championshipId,
        int round)
    {
        var cacheKey = $"Leaderboard-{blockIndex}-{championshipId}-{round}";
        if (_leaderboardCache.TryGetValue(cacheKey, out List<ArenaRankingDocument>? leaderboard))
        {
            return leaderboard!;
        }

        await _leaderboardCacheLock.WaitAsync();
        try
        {
            if (!_leaderboardCache.TryGetValue(cacheKey, out leaderboard))
            {
                var collection = dbService.GetCollection<ArenaDocument>(CollectionNames.Arena);
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
                            { "SimpleAvatar.Object.EventMap", 0 },
                            { "SimpleAvatar.Object.ItemMap", 0 },
                            { "SimpleAvatar.Object.MailBox", 0 },
                            { "SimpleAvatar.Object.MonsterMap", 0 },
                            { "SimpleAvatar.Object.StageMap", 0 },
                        }
                    ),
                    new(
                        "$addFields",
                        new BsonDocument
                        {
                            { "SimpleAvatar", "$SimpleAvatar.Object" },
                        }
                    ),
                    new("$sort", new BsonDocument("ArenaScore.Score", -1)),
                };

                var aggregation = await collection.Aggregate<ArenaRankingDocument>(pipelines).ToListAsync();
                leaderboard = UpdateRank(aggregation);
                _leaderboardCache.Set(cacheKey, leaderboard, CacheEntryOptions);
            }
        }
        finally
        {
            _leaderboardCacheLock.Release();
        }

        return leaderboard!;
    }

    private static List<ArenaRankingDocument> UpdateRank(List<ArenaRankingDocument> source)
    {
        int? currentScore = null;
        var currentRank = 1;
        var trunk = new List<ArenaRankingDocument>();
        var result = new List<ArenaRankingDocument>();
        var count = source.Count;
        var lastIndex = count - 1;
        for (var i = 0; i < count; i++)
        {
            var doc = source[i];
            if (currentScore is null)
            {
                currentScore = doc.ArenaScore.Score;
                trunk.Add(doc);
                continue;
            }

            if (doc.ArenaScore.Score == currentScore)
            {
                currentRank++;
                trunk.Add(doc);
                if (i < lastIndex)
                {
                    continue;
                }

                TrunkToResult(trunk, currentRank, result);
                break;
            }

            TrunkToResult(trunk, currentRank, result);
            if (i < lastIndex)
            {
                currentScore = doc.ArenaScore.Score;
                currentRank++;
                trunk.Add(doc);
                continue;
            }

            doc.Rank = currentRank + 1;
            result.Add(doc);
            break;
        }

        return result;

        void TrunkToResult(List<ArenaRankingDocument> t, int rank, List<ArenaRankingDocument> r)
        {
            foreach (var inTrunk in t.OrderBy(e => e.Address))
            {
                inTrunk.Rank = rank;
                r.Add(inTrunk);
            }

            t.Clear();
        }
    }

    public async Task<List<ArenaRankingDocument>> GetLeaderboardAsync(
        long blockIndex,
        int championshipId,
        int round,
        int rank,
        int count)
    {
        var leaderboard = await GetOrCreateLeaderboardAsync(blockIndex, championshipId, round);
        return leaderboard
            .SkipWhile(e => e.Rank < rank)
            .Take(count)
            .ToList();
    }

    public async Task<int?> GetRankingByAvatarAddressAsync(
        long blockIndex,
        int championshipId,
        int round,
        Address avatarAddress)
    {
        var leaderboard = await GetOrCreateLeaderboardAsync(blockIndex, championshipId, round);
        var doc = leaderboard.FirstOrDefault(e => e.Address.Equals(avatarAddress));
        return doc?.Rank;
    }

    public async Task<List<ArenaRankingDocument>> GetLeaderboardByAvatarAddressAsync(
        long blockIndex,
        int championshipId,
        int round,
        ArenaType arenaType,
        Address avatarAddress)
    {
        var leaderboard = await GetOrCreateLeaderboardAsync(blockIndex, championshipId, round);
        var doc = leaderboard.FirstOrDefault(e =>
            e.ChampionshipId == championshipId &&
            e.Round == round &&
            e.Address.Equals(avatarAddress));
        if (doc is null)
        {
            return await GetLeaderboardByScoreAsync(
                blockIndex,
                championshipId,
                round,
                arenaType,
                ArenaScore.ArenaScoreDefault);
        }
    
        return await GetLeaderboardByScoreAsync(
            blockIndex,
            championshipId,
            round,
            arenaType,
            doc.ArenaScore.Score);
    }
    
    public async Task<List<ArenaRankingDocument>> GetLeaderboardByScoreAsync(
        long blockIndex,
        int championshipId,
        int round,
        ArenaType arenaType,
        int score)
    {
        var leaderboard = await GetOrCreateLeaderboardAsync(blockIndex, championshipId, round);
        var bounds = ArenaHelper.ScoreLimits.TryGetValue(arenaType, out var limit)
            ? limit
            : ArenaHelper.ScoreLimits.First().Value;
        var upperBound = bounds.upper + score;
        var lowerBound = bounds.lower + score;
        return leaderboard
            .SkipWhile(e => e.ArenaScore.Score > upperBound)
            .TakeWhile(e => e.ArenaScore.Score >= lowerBound)
            .ToList();
    }
}
