using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Models.Arena;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ArenaRepository(
    MongoDbService dbService,
    AvatarRepository avatarRepository)
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

        var aggregation = await collection
            .Aggregate<dynamic>(pipelines)
            .ToListAsync();
        return aggregation.Count == 0
            ? 0
            : (long)aggregation.First().Rank + 1;
    }

    public async Task<List<ArenaRanking>> GetLeaderboardAsync(
        long skip,
        int limit,
        int championshipId,
        int round)
    {
        var collection = dbService.GetCollection<ArenaDocument>(CollectionNames.Arena.Value);
        return await GetLeaderboardAsync(collection, skip, limit, championshipId, round);
    }

    private async Task<List<ArenaRanking>> GetLeaderboardAsync(
        IMongoCollection<ArenaDocument> collection,
        long skip,
        int limit,
        int championshipId,
        int round)
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
            new("$limit", limit)
        };

        var aggregation = collection
            .Aggregate<ArenaDocument>(pipelines)
            .ToList();
        var arenaRankings = await Task.WhenAll(aggregation
            .Where(e => e is not null)
            .Select(BuildArenaRankingFromDocument));
        return UpdateRank(arenaRankings
            .Where(e => e is not null)
            .ToList()!);
    }

    private async Task<ArenaRanking?> BuildArenaRankingFromDocument(ArenaDocument document)
    {
        var arenaRanking = new ArenaRanking(
            document.AvatarAddress.ToHex(),
            document.ArenaInformation.Address.ToHex(),
            document.ArenaInformation.Win,
            document.ArenaInformation.Lose,
            document.ArenaInformation.Ticket,
            document.ArenaInformation.TicketResetCount,
            document.ArenaInformation.PurchasedTicketCount,
            document.ArenaScore.Score)
        {
            Rank = document.ExtraElements?["Rank"].ToInt64() + 1 ?? 0,
        };

        try
        {
            // Set Avatar
            var doc = await avatarRepository.GetByAddressAsync(document.AvatarAddress);
            arenaRanking.Avatar = new Avatar(doc.Object);

            // NOTE: Uncomment if the CP needed.
            // // Set CP
            // var runeSlotState = await _stateGetter.GetArenaRuneSlotStateAsync(
            //     document.AvatarAddress);
            // var cp = await _cpRepository.CalculateCp(
            //     avatar,
            //     runeSlotState);
            // arenaRanking.CP = cp;
        }
        catch (DocumentNotFoundInMongoCollectionException e)
        {
            Console.WriteLine(e.Message);
        }

        return arenaRanking;
    }

    private static List<ArenaRanking> UpdateRank(List<ArenaRanking> source)
    {
        int? currentScore = null;
        var currentRank = 1;
        var trunk = new List<ArenaRanking>();
        var result = new List<ArenaRanking>();
        for (var i = 0; i < source.Count; i++)
        {
            var arenaRanking = source[i];
            if (!currentScore.HasValue)
            {
                currentScore = arenaRanking.Score;
                trunk.Add(arenaRanking);
                continue;
            }

            if (currentScore.Value == arenaRanking.Score)
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
                currentScore = arenaRanking.Score;
                currentRank++;
                continue;
            }

            arenaRanking.Rank = currentRank + 1;
            result.Add(arenaRanking);
        }

        return result;

        void TrunkToResult()
        {
            if (trunk.Count == 0)
            {
                result.Add(trunk[0]);
            }
            else
            {
                foreach (var inTrunk in trunk.OrderBy(e => new Address(e.AvatarAddress)))
                {
                    inTrunk.Rank = currentRank;
                    result.Add(inTrunk);
                }    
            }

            trunk.Clear();
        }
    }
}
