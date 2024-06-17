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
                        new BsonDocument("State.RoundData.ChampionshipId", championshipId),
                        new BsonDocument("State.RoundData.Round", round)
                    }
                )
            ),
            new BsonDocument(
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
            new BsonDocument("$skip", offset),
            new BsonDocument("$limit", limit),
            new BsonDocument(
                "$lookup",
                new BsonDocument
                {
                    { "from", "avatar" },
                    { "localField", "_id" },
                    { "foreignField", "ArenaObjectId" },
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
