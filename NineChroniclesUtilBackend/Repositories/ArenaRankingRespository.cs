using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.State;
using NineChroniclesUtilBackend.Models.Agent;
using NineChroniclesUtilBackend.Models.Arena;
using NineChroniclesUtilBackend.Services;

namespace NineChroniclesUtilBackend.Repositories;

public class ArenaRankingRepository(MongoDBCollectionService mongoDBCollectionService)
{
    private readonly IMongoCollection<dynamic> ArenaCollection =
        mongoDBCollectionService.GetCollection<dynamic>("arena");

    public async Task<long> GetRankByAvatarAddress(string avatarAddress)
    {
        var pipelines = new BsonDocument[]
        {
            new("$sort", new BsonDocument("Score.Score", -1)),
            new(
                "$group",
                new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "docs", new BsonDocument("$push", "$$ROOT") },
                }
            ),
            new(
                "$unwind",
                new BsonDocument { { "path", "$docs" }, { "includeArrayIndex", "Rank" }, }
            ),
            new("$match", new BsonDocument("docs.AvatarAddress", avatarAddress)),
        };

        var aggregation = await ArenaCollection.Aggregate<dynamic>(pipelines).ToListAsync();

        return aggregation.First().Rank;
    }

    public async Task<List<ArenaRanking>> GetRanking(long limit, long offset)
    {
        var pipelines = new BsonDocument[]
        {
            new("$sort", new BsonDocument("Score.Score", -1)),
            new(
                "$group",
                new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "docs", new BsonDocument("$push", "$$ROOT") },
                }
            ),
            new(
                "$unwind",
                new BsonDocument { { "path", "$docs" }, { "includeArrayIndex", "Rank" }, }
            ),
            new("$skip", offset),
            new("$limit", limit),
            new(
                "$replaceRoot",
                new BsonDocument
                {
                    {
                        "newRoot",
                        new BsonDocument
                        {
                            {
                                "$mergeObjects",
                                new BsonArray { "$docs", new BsonDocument("Rank", "$Rank"), }
                            },
                        }
                    },
                }
            ),
            new(
                "$lookup",
                new BsonDocument
                {
                    { "from", "avatars" },
                    { "localField", "AvatarAddress" },
                    { "foreignField", "Avatar.address" },
                    { "as", "Avatar" },
                }
            ),
            new(
                "$unwind",
                new BsonDocument { { "path", "$Avatar" }, { "preserveNullAndEmptyArrays", true } }
            ),
        };

        var aggregation = await ArenaCollection.Aggregate<dynamic>(pipelines).ToListAsync();

        var result = aggregation
            .Select(x =>
            {
                var equipments = ((List<object>)x.Avatar.ItemSlot.Equipments).OfType<string>();
                var costumes = ((List<object>)x.Avatar.ItemSlot.Costumes).OfType<string>();
                var runeSlots = ((List<dynamic>)x.Avatar.RuneSlot).Select(rune =>
                    ((int)rune.RuneId, (int)rune.Level)
                );

                var cp = CpRepository
                    .CalculateCp(
                        x.Avatar.Avatar.address,
                        x.Avatar.Avatar.level,
                        x.Avatar.Avatar.characterId,
                        equipments,
                        costumes,
                        runeSlots
                    )
                    .Result;

                return new ArenaRanking(
                    x.AvatarAddress,
                    x.Information.Address,
                    cp,
                    x.Information.Win,
                    x.Information.Lose,
                    x.Rank + 1,
                    x.Information.Ticket,
                    x.Information.TicketResetCount,
                    x.Information.PurchasedTicketCount,
                    x.Score.Score,
                    new Avatar(x.Avatar.Avatar.address, x.Avatar.Avatar.name, x.Avatar.Avatar.level)
                );
            })
            .ToList();

        return result;
    }
}
