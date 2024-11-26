using Libplanet.Crypto;
using Microsoft.Extensions.Caching.Memory;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Arena;
using Nekoyume.Model.Arena;
using Nekoyume.Model.EnumType;

namespace Mimir.MongoDB.Repositories;

public class ArenaParticipantRepository(IMongoDbService dbService)
{
    private static readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetPriority(CacheItemPriority.High)
        .SetSlidingExpiration(TimeSpan.FromMinutes(1));

    private readonly MemoryCache _leaderboardCache = new(new MemoryCacheOptions());
    private readonly SemaphoreSlim _leaderboardCacheLock = new(1, 1);

    private readonly IMongoCollection<ArenaParticipantDocument> _collection = dbService
        .GetCollection<ArenaParticipantDocument>(CollectionNames.GetCollectionName<ArenaParticipantDocument>());

    public async Task<ArenaParticipantDocument> GetByAddressAsync(Address avatarAddress)
    {
        var filter = Builders<ArenaParticipantDocument>.Filter.Eq("_id", avatarAddress.ToHex());
        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                _collection.CollectionNamespace.CollectionName,
                $"'_id' equals to '{avatarAddress.ToHex()}'");
        }

        return document;
    }

    public async Task<List<ArenaParticipantDocument>> GetLeaderboardAsync(
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

    public async Task<List<ArenaParticipantDocument>> GetLeaderboardByAvatarAddressAsync(
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
            e.Id.Equals(avatarAddress));
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
            doc.Object.Score);
    }

    public async Task<List<ArenaParticipantDocument>> GetLeaderboardByScoreAsync(
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
            .SkipWhile(e => e.Object.Score > upperBound)
            .TakeWhile(e => e.Object.Score >= lowerBound)
            .ToList();
    }

    public async Task<int?> GetRankingByAddressAsync(
        long blockIndex,
        int championshipId,
        int round,
        Address avatarAddress)
    {
        var leaderboard = await GetOrCreateLeaderboardAsync(blockIndex, championshipId, round);
        var doc = leaderboard.FirstOrDefault(e => e.Id.Equals(avatarAddress));
        return doc?.Rank;
    }

    private async Task<List<ArenaParticipantDocument>> GetOrCreateLeaderboardAsync(
        long blockIndex,
        int championshipId,
        int round)
    {
        var cacheKey = $"Leaderboard-{blockIndex}-{championshipId}-{round}";
        if (_leaderboardCache.TryGetValue(cacheKey, out List<ArenaParticipantDocument>? leaderboard))
        {
            return leaderboard!;
        }

        await _leaderboardCacheLock.WaitAsync();
        try
        {
            if (_leaderboardCache.TryGetValue(cacheKey, out leaderboard))
            {
                return leaderboard!;
            }

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
                new("$sort", new BsonDocument("Object.Score", -1)),
            };

            var aggregation = await _collection.Aggregate<ArenaParticipantDocument>(pipelines).ToListAsync();
            leaderboard = UpdateRank(aggregation);
            _leaderboardCache.Set(cacheKey, leaderboard, CacheEntryOptions);
        }
        finally
        {
            _leaderboardCacheLock.Release();
        }

        return leaderboard;
    }

    private static List<ArenaParticipantDocument> UpdateRank(List<ArenaParticipantDocument> source)
    {
        int? currentScore = null;
        var currentRank = 1;
        var trunk = new List<ArenaParticipantDocument>();
        var result = new List<ArenaParticipantDocument>();
        var count = source.Count;
        var lastIndex = count - 1;
        for (var i = 0; i < count; i++)
        {
            var doc = source[i];
            if (currentScore is null)
            {
                currentScore = doc.Object.Score;
                trunk.Add(doc);
                continue;
            }

            if (doc.Object.Score == currentScore)
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
                currentScore = doc.Object.Score;
                currentRank++;
                trunk.Add(doc);
                continue;
            }

            doc.Rank = currentRank + 1;
            result.Add(doc);
            break;
        }

        return result;

        void TrunkToResult(List<ArenaParticipantDocument> t, int rank, List<ArenaParticipantDocument> r)
        {
            foreach (var inTrunk in t.OrderBy(e => e.Id))
            {
                inTrunk.Rank = rank;
                r.Add(inTrunk);
            }

            t.Clear();
        }
    }
}
