using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Models.Arena;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ArenaRankingRepository : BaseRepository<BsonDocument>
{
    private readonly CpRepository _cpRepository;

    public ArenaRankingRepository(
        MongoDBCollectionService mongoDBCollectionService,
        IStateService stateService
    )
        : base(mongoDBCollectionService)
    {
        _cpRepository = new CpRepository(stateService);
    }

    protected override string GetCollectionName()
    {
        return "arena";
    }

    public async Task<long> GetRankByAvatarAddress(
        string network,
        Address avatarAddress,
        int? championshipId,
        int? round
    )
    {
        var collection = GetCollection(network);

        var pipelines = new BsonDocument[]
        {
            new BsonDocument(
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
            new("$sort", new BsonDocument("Score.Score", -1)),
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
        string network,
        long limit,
        long offset,
        int? championshipId,
        int? round
    )
    {
        var collection = GetCollection(network);

        var pipelines = new List<BsonDocument>
        {
            new BsonDocument(
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
            new BsonDocument(
                "$setWindowFields",
                new BsonDocument
                {
                    { "partitionBy", "" },
                    { "sortBy", new BsonDocument("Score.Score", -1) },
                    {
                        "output",
                        new BsonDocument("Rank", new BsonDocument("$rank", new BsonDocument()))
                    }
                }
            ),
            new BsonDocument("$skip", offset),
            new BsonDocument("$limit", limit),
            new BsonDocument(
                "$lookup",
                new BsonDocument
                {
                    { "from", "avatars" },
                    { "localField", "AvatarAddress" },
                    { "foreignField", "Avatar.address" },
                    { "as", "Avatar" }
                }
            ),
            new BsonDocument(
                "$unwind",
                new BsonDocument { { "path", "$Avatar" }, { "preserveNullAndEmptyArrays", true } }
            ),
            new BsonDocument(
                "$unset",
                new BsonArray
                {
                    "Avatar.Avatar.inventory",
                    "Avatar.Avatar.mailBox",
                    "Avatar.Avatar.stageMap",
                    "Avatar.Avatar.monsterMap",
                    "Avatar.Avatar.itemMap",
                    "Avatar.Avatar.eventMap"
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
        var arenaRanking = new ArenaRanking(
            document["AvatarAddress"].AsString,
            document["Information"]["Address"].AsString,
            document["Information"]["Win"].AsInt32,
            document["Information"]["Lose"].AsInt32,
            document["Rank"].AsInt32 + 1,
            document["Information"]["Ticket"].AsInt32,
            document["Information"]["TicketResetCount"].AsInt32,
            document["Information"]["PurchasedTicketCount"].AsInt32,
            document["Score"]["Score"].AsInt32
        );

        if (!document.Contains("Avatar"))
        {
            return arenaRanking;
        }

        var avatar = new Avatar(
            document["Avatar"]["Avatar"]["agentAddress"].AsString,
            document["Avatar"]["Avatar"]["address"].AsString,
            document["Avatar"]["Avatar"]["name"].AsString,
            document["Avatar"]["Avatar"]["level"].AsInt32,
            document["Avatar"]["Avatar"]["actionPoint"].AsInt32,
            document["Avatar"]["Avatar"]["dailyRewardReceivedIndex"].AsInt64
        );
        arenaRanking.Avatar = avatar;

        var characterId = document["Avatar"]["Avatar"]["characterId"].AsInt32;
        var equipmentIds = document["Avatar"]
            ["ItemSlot"]["Equipments"]
            .AsBsonArray.Select(x => x.AsString);
        var costumeIds = document["Avatar"]
            ["ItemSlot"]["Costumes"]
            .AsBsonArray.Select(x => x.AsString);
        var runeSlots = document["Avatar"]
            ["RuneSlot"]
            .AsBsonArray.Select(rune => (rune["RuneId"].AsInt32, rune["Level"].AsInt32));

        var cp = await _cpRepository.CalculateCp(
            avatar,
            characterId,
            equipmentIds,
            costumeIds,
            runeSlots
        );
        arenaRanking.CP = cp;
        Console.WriteLine($"CP Calculate {arenaRanking.ArenaAddress}");

        return arenaRanking;
    }
}
