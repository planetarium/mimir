using Bencodex;
using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models.Arena;
using Mimir.Services;
using Mimir.Util;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ArenaRepository(MongoDbService dbService, IStateService stateService)
{
    private static readonly Codec Codec = new();
    private readonly CpRepository _cpRepository = new(stateService);
    private StateGetter _stateGetter = new(stateService);

    public async Task<long> GetRankingByAvatarAddressAsync(
        Address avatarAddress,
        int championshipId,
        int round
    )
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.Arena.Value);
        return await GetRankingByAvatarAddressAsync(
            collection,
            avatarAddress,
            championshipId,
            round
        );
    }

    private static async Task<long> GetRankingByAvatarAddressAsync(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress,
        int championshipId,
        int round
    )
    {
        var pipelines = new BsonDocument[]
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("RoundData.ChampionshipId", championshipId),
                        new BsonDocument("RoundData.Round", round)
                    }
                )
            ),
            new("$sort", new BsonDocument("Object.Score", -1)),
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
            new("$match", new BsonDocument("docs.AvatarAddress", avatarAddress.ToHex()))
        };

        var aggregation = await collection.Aggregate<dynamic>(pipelines).ToListAsync();
        return aggregation.Count == 0 ? 0 : (long)aggregation.First().Rank;
    }

    public async Task<List<ArenaRanking>> GetRanking(
        long skip,
        int limit,
        int championshipId,
        int round
    )
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.Arena.Value);
        return await GetRanking(collection, skip, limit, championshipId, round);
    }

    private async Task<List<ArenaRanking>> GetRanking(
        IMongoCollection<BsonDocument> collection,
        long skip,
        int limit,
        int? championshipId,
        int? round
    )
    {
        var pipelines = new List<BsonDocument>
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("RoundData.ChampionshipId", championshipId),
                        new BsonDocument("RoundData.Round", round)
                    }
                )
            ),
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
                new BsonDocument { { "path", "$docs" }, { "includeArrayIndex", "docs.Rank" } }
            ),
            new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$docs")),
            new("$skip", skip),
            new("$limit", limit)
        };

        var aggregation = collection.Aggregate<BsonDocument>(pipelines).ToList();
        var arenaRankings = await Task.WhenAll(
            aggregation.Where(e => e is not null).Select(BuildArenaRankingFromDocument));
        return arenaRankings.Where(e => e is not null).ToList()!;
    }

    private async Task<ArenaRanking?> BuildArenaRankingFromDocument(BsonDocument document)
    {
        var avatarAddress = document["AvatarAddress"].AsString;

        var arenaRanking = new ArenaRanking(
            document["AvatarAddress"].AsString,
            document["ArenaInformationObject"]["Address"].AsString,
            document["ArenaInformationObject"]["Win"].ToInt32(),
            document["ArenaInformationObject"]["Lose"].ToInt32(),
            document["Rank"].ToInt64() + 1,
            document["ArenaInformationObject"]["Ticket"].ToInt32(),
            document["ArenaInformationObject"]["TicketResetCount"].ToInt32(),
            document["ArenaInformationObject"]["PurchasedTicketCount"].ToInt32(),
            document["ArenaScoreObject"]["Score"].ToInt32()
        );

        try
        {
            var avatar = AvatarRepository.GetAvatar(
                dbService.GetCollection<BsonDocument>(CollectionNames.Avatar.Value),
                new Address(avatarAddress)
            );

            arenaRanking.Avatar = avatar;

            var runeSlotState = await _stateGetter.GetArenaRuneSlotStateAsync(
                new Address(avatarAddress)
            );
            if (runeSlotState is null)
            {
                return arenaRanking;
            }

            var cp = await _cpRepository.CalculateCp(
                avatar,
                runeSlotState
            );
            arenaRanking.CP = cp;
            Console.WriteLine($"CP Calculate {arenaRanking.ArenaAddress}");
        }
        catch (DocumentNotFoundInMongoCollectionException e)
        {
            Console.WriteLine(e.Message);
        }

        return arenaRanking;
    }
}
