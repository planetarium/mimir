using System.Text;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.GraphQL.Extensions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;

namespace Mimir.Repositories;

public class TableSheetsRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    protected override string GetCollectionName() => "table_sheet";

    public ArenaSheet.RoundData GetArenaRound(long blockIndex)
    {
        var collection = GetCollection();
        return GetArenaRound(collection, blockIndex);
    }

    private static ArenaSheet.RoundData GetArenaRound(
        IMongoCollection<BsonDocument> collection,
        long blockIndex)
    {
        var pipelines = new BsonDocument[]
        {
            new("$match", new BsonDocument("State.Name", "ArenaSheet")),
            new(
                "$addFields",
                new BsonDocument("SheetJsonArray", new BsonDocument("$objectToArray", "$State.Object"))
            ),
            new("$unwind", new BsonDocument("path", "$SheetJsonArray")),
            new("$unwind", new BsonDocument("path", "$SheetJsonArray.v.Round")),
            new(
                "$match",
                new BsonDocument(
                    "$and",
                    new BsonArray
                    {
                        new BsonDocument(
                            "SheetJsonArray.v.Round.StartBlockIndex",
                            new BsonDocument("$lte", blockIndex)
                        ),
                        new BsonDocument(
                            "SheetJsonArray.v.Round.EndBlockIndex",
                            new BsonDocument("$gte", blockIndex)
                        )
                    }
                )
            ),
            new(
                "$project",
                new BsonDocument
                {
                    { "Round", "$SheetJsonArray.v.Round" },
                }
            )
        };
        var document = collection.Aggregate<BsonDocument>(pipelines).FirstOrDefault();
        if (document == null)
        {
            throw new InvalidOperationException("No matching document found.");
        }

        try
        {
            var roundDoc = document["Round"];
            return new ArenaSheet.RoundData(
                roundDoc["ChampionshipId"].AsInt32,
                roundDoc["Round"].AsInt32,
                (ArenaType)roundDoc["ArenaType"].AsInt32,
                roundDoc["StartBlockIndex"].ToLong(),
                roundDoc["EndBlockIndex"].ToLong(),
                roundDoc["RequiredMedalCount"].AsInt32,
                roundDoc["EntranceFee"].AsInt32,
                roundDoc["TicketPrice"].AsInt32,
                roundDoc["AdditionalTicketPrice"].AsInt32,
                roundDoc["MaxPurchaseCount"].AsInt32,
                roundDoc["MaxPurchaseCountWithInterval"].AsInt32);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(null, e);
        }
        catch (InvalidCastException e)
        {
            throw new UnexpectedTypeOfBsonValueException(null, e);
        }
    }

    public string[] GetSheetNames()
    {
        var collection = GetCollection();
        var projection = Builders<BsonDocument>.Projection.Include("State.Name").Exclude("_id");
        var documents = collection.Find(new BsonDocument()).Project(projection).ToList();

        var names = new List<string>();
        foreach (var document in documents)
        {
            if (document.Contains("State") &&
                document["State"].AsBsonDocument.Contains("Name"))
            {
                names.Add(document["State"]["Name"].AsString);
            }
        }

        return names.ToArray();
    }

    public async Task<string> GetSheetAsync(
        string sheetName,
        SheetFormat sheetFormat = SheetFormat.Csv)
    {
        var collection = GetCollection();
        var database = GetDatabase();
        return await GetSheetAsync(collection, database, sheetName, sheetFormat);
    }

    public async Task<T> GetSheetAsync<T>(
        SheetFormat sheetFormat = SheetFormat.Csv) where T : ISheet
    {
        var sheetType = typeof(T);
        var csv = await GetSheetAsync(sheetType.Name, sheetFormat);
        var sheetConstructorInfo = sheetType.GetConstructor(Type.EmptyTypes);
        if (sheetConstructorInfo?.Invoke([]) is not ISheet sheet)
        {
            throw new FailedLoadSheetException(sheetType);
        }

        sheet.Set(csv);
        return (T)sheet;
    }

    private static async Task<string> GetSheetAsync(
        IMongoCollection<BsonDocument> collection,
        IMongoDatabase database,
        string sheetName,
        SheetFormat sheetFormat)
    {
        var gridFs = new GridFSBucket(database);
        var fieldToInclude = sheetFormat switch
        {
            SheetFormat.Csv => "SheetCsvFileId",
            SheetFormat.Json => "SheetJson",
            _ => "SheetJson"
        };

        var projection = Builders<BsonDocument>.Projection.Include(fieldToInclude).Exclude("_id");
        var filter = Builders<BsonDocument>.Filter.Eq("State.Name", sheetName);
        var document = collection.Find(filter).Project(projection).FirstOrDefault();

        if (
            document == null
            || !document.Contains(fieldToInclude)
            || document[fieldToInclude].IsBsonNull
        )
        {
            throw new KeyNotFoundException(sheetName);
        }

        return sheetFormat switch
        {
            SheetFormat.Csv => await RetrieveFromGridFs(gridFs, document[fieldToInclude].AsObjectId),
            _ => document[fieldToInclude].ToJson(new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.CanonicalExtendedJson
            })
        };
    }

    private static async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }
}
