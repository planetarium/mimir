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

    public async Task<BsonDocument> GetArenaInformationAsync(
        Address avatarAddress,
        int championshipId,
        int round
    ) =>
        await GetArenaInformationAsync(
            dbService.GetCollection<BsonDocument>(CollectionNames.ArenaInformation.Value),
            avatarAddress,
            championshipId,
            round
        );

    public async Task<BsonDocument> GetArenaInformationAsync(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress,
        int championshipId,
        int round
    )
    {
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("State.AvatarAddress", avatarAddress.ToHex());
        filter &= builder.Eq("State.RoundData.ChampionshipId", championshipId);
        filter &= builder.Eq("State.RoundData.Round", round);
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'"
            );
        }

        try
        {
            var arenaInformationDoc = document["State"].AsBsonDocument;
            return arenaInformationDoc;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("document[\"State\"]", e);
        }
    }

    public async Task<long> GetRankingByAvatarAddressAsync(
        Address avatarAddress,
        int championshipId,
        int round
    )
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.ArenaScore.Value);
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
                        new BsonDocument("State.RoundData.ChampionshipId", championshipId),
                        new BsonDocument("State.RoundData.Round", round)
                    }
                )
            ),
            new("$sort", new BsonDocument("State.Object.Score", -1)),
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
        long skip,
        int limit,
        int championshipId,
        int round
    )
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.ArenaScore.Value);
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
                        new BsonDocument("State.RoundData.ChampionshipId", championshipId),
                        new BsonDocument("State.RoundData.Round", round)
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
        var avatarAddress = document["State"]["AvatarAddress"].AsString;
        BsonDocument arenaInformationDoc;
        try
        {
            arenaInformationDoc = await GetArenaInformationAsync(
                new Address(avatarAddress),
                document["State"]["RoundData"]["ChampionshipId"].ToInt32(),
                document["State"]["RoundData"]["Round"].ToInt32()
            );
        }
        catch (DocumentNotFoundInMongoCollectionException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

        var arenaRanking = new ArenaRanking(
            document["State"]["AvatarAddress"].AsString,
            arenaInformationDoc["Object"]["Address"].AsString,
            arenaInformationDoc["Object"]["Win"].ToInt32(),
            arenaInformationDoc["Object"]["Lose"].ToInt32(),
            document["Rank"].ToInt64() + 1,
            arenaInformationDoc["Object"]["Ticket"].ToInt32(),
            arenaInformationDoc["Object"]["TicketResetCount"].ToInt32(),
            arenaInformationDoc["Object"]["PurchasedTicketCount"].ToInt32(),
            document["State"]["Object"]["Score"].ToInt32()
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
        catch (DocumentNotFoundInMongoCollectionException)
        {
        }

        return arenaRanking;
    }
}
