using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Models.Arena;
using Mimir.Services;
using Mimir.Util;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ArenaRankingRepository(
    MongoDBCollectionService mongoDbCollectionService,
    IStateService stateService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    private readonly CpRepository _cpRepository = new(stateService);
    private StateGetter _stateGetter = new(stateService);

    protected override string GetCollectionName() => "arena";

    public async Task<long> GetRankingByAvatarAddressAsync(
        string network,
        Address avatarAddress,
        int? championshipId,
        int? round)
    {
        var collection = GetCollection(network);
        return await GetRankingByAvatarAddressAsync(collection, avatarAddress, championshipId, round);
    }

    public async Task<long> GetRankingByAvatarAddressAsync(
        PlanetName planetName,
        Address avatarAddress,
        int? championshipId,
        int? round)
    {
        var collection = GetCollection(planetName);
        return await GetRankingByAvatarAddressAsync(collection, avatarAddress, championshipId, round);
    }

    private static async Task<long> GetRankingByAvatarAddressAsync(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress,
        int? championshipId,
        int? round)
    {
        var pipelines = new BsonDocument[]
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("State.RoundData.ChampionshipId", championshipId),
                        new BsonDocument("State.RoundData.Round", round)
                    }
                )
            ),
            new("$sort", new BsonDocument("State.Score", -1)),
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
            new("$match", new BsonDocument("docs.State.AvatarAddress", avatarAddress.ToHex()))
        };

        var aggregation = await collection.Aggregate<dynamic>(pipelines).ToListAsync();
        return aggregation.Count == 0 ? 0 : (long)aggregation.First().Rank;
    }

    public async Task<List<ArenaRanking>> GetRanking(
        string network,
        long skip,
        int limit,
        int? championshipId,
        int? round)
    {
        var collection = GetCollection(network);
        return await GetRanking(collection, skip, limit, championshipId, round);
    }

    public async Task<List<ArenaRanking>> GetRanking(
        PlanetName planetName,
        long skip,
        int limit,
        int? championshipId,
        int? round)
    {
        var collection = GetCollection(planetName);
        return await GetRanking(collection, skip, limit, championshipId, round);
    }

    private async Task<List<ArenaRanking>> GetRanking(
        IMongoCollection<BsonDocument> collection,
        long skip,
        int limit,
        int? championshipId,
        int? round)
    {
        var pipelines = new List<BsonDocument>
        {
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument("State.RoundData.ChampionshipId", championshipId),
                        new BsonDocument("State.RoundData.Round", round)
                    }
                )
            ),
            new(
                "$setWindowFields",
                new BsonDocument
                {
                    { "partitionBy", "" },
                    { "sortBy", new BsonDocument("State.ArenaScoreObject.Score", -1) },
                    {
                        "output",
                        new BsonDocument("Rank", new BsonDocument("$rank", new BsonDocument()))
                    }
                }
            ),
            new("$skip", skip),
            new("$limit", limit),
            new(
                "$lookup",
                new BsonDocument
                {
                    { "from", "avatar" },
                    { "localField", "_id" },
                    { "foreignField", "ArenaObjectId" },
                    { "as", "Avatar" }
                }
            ),
            new(
                "$unwind",
                new BsonDocument { { "path", "$Avatar" }, { "preserveNullAndEmptyArrays", true } }
            ),
            new(
                "$unset",
                new BsonArray
                {
                    "Avatar.State.inventory",
                    "Avatar.State.mailBox",
                    "Avatar.State.stageMap",
                    "Avatar.State.monsterMap",
                    "Avatar.State.itemMap",
                    "Avatar.State.eventMap"
                }
            )
        };

        var aggregation = collection.Aggregate<BsonDocument>(pipelines).ToList();
        var arenaRankings = await Task.WhenAll(
            aggregation.OfType<BsonDocument>().Select(BuildArenaRankingFromDocument)
        );
        return arenaRankings.ToList();
    }

    private async Task<ArenaRanking> BuildArenaRankingFromDocument(BsonDocument document)
    {
        var avatarAddress = document["State"]["AvatarAddress"].AsString;
        var arenaRanking = new ArenaRanking(
            document["State"]["AvatarAddress"].AsString,
            document["State"]["ArenaInformationObject"]["Address"].AsString,
            document["State"]["ArenaInformationObject"]["Win"].AsInt32,
            document["State"]["ArenaInformationObject"]["Lose"].AsInt32,
            document["Rank"].AsInt32 + 1,
            document["State"]["ArenaInformationObject"]["Ticket"].AsInt32,
            document["State"]["ArenaInformationObject"]["TicketResetCount"].AsInt32,
            document["State"]["ArenaInformationObject"]["PurchasedTicketCount"].AsInt32,
            document["State"]["ArenaScoreObject"]["Score"].AsInt32
        );

        if (!document.Contains("Avatar"))
        {
            return arenaRanking;
        }

        var avatar = new Avatar(
            document["Avatar"]["State"]["agentAddress"].AsString,
            document["Avatar"]["State"]["address"].AsString,
            document["Avatar"]["State"]["name"].AsString,
            document["Avatar"]["State"]["level"].AsInt32,
            document["Avatar"]["State"]["actionPoint"].AsInt32,
            document["Avatar"]["State"]["dailyRewardReceivedIndex"].ToInt64()
        );
        arenaRanking.Avatar = avatar;

        var characterId = document["Avatar"]["State"]["characterId"].AsInt32;

        var itemSlotState = await _stateGetter.GetItemSlotStateAsync(new Address(avatarAddress));
        if (itemSlotState is null)
        {
            return arenaRanking;
        }

        var runeSlotState = await _stateGetter.GetArenaRuneSlotStateAsync(
            new Address(avatarAddress)
        );
        if (runeSlotState is null)
        {
            return arenaRanking;
        }

        var equipmentIds = itemSlotState.Equipments;
        var costumeIds = itemSlotState.Costumes;

        var cp = await _cpRepository.CalculateCp(
            avatar,
            characterId,
            equipmentIds,
            costumeIds,
            runeSlotState
        );
        arenaRanking.CP = cp;
        Console.WriteLine($"CP Calculate {arenaRanking.ArenaAddress}");

        return arenaRanking;
    }
}
