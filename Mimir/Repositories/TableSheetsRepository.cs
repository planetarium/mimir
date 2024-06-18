using System.Text;
using Mimir.Enums;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.Repositories;

public class TableSheetsRepository : BaseRepository<BsonDocument>
{
    public TableSheetsRepository(MongoDBCollectionService mongoDBCollectionService)
        : base(mongoDBCollectionService) { }

    protected override string GetCollectionName()
    {
        return "table_sheet";
    }

    public async Task<(int, int)> GetLatestArenaSeason(string network, long blockIndex)
    {
        var collection = GetCollection<BsonDocument>(network);

        var pipelines = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("State.Name", "ArenaSheet")),
            new BsonDocument(
                "$addFields",
                new BsonDocument("SheetJsonArray", new BsonDocument("$objectToArray", "$State.Object"))
            ),
            new BsonDocument("$unwind", new BsonDocument("path", "$SheetJsonArray")),
            new BsonDocument("$unwind", new BsonDocument("path", "$SheetJsonArray.v.Round")),
            new BsonDocument(
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
            new BsonDocument(
                "$project",
                new BsonDocument
                {
                    { "_id", 0 },
                    { "ChampionshipId", "$SheetJsonArray.v.Round.ChampionshipId" },
                    { "Round", "$SheetJsonArray.v.Round.Round" }
                }
            )
        };

        var document = collection.Aggregate<BsonDocument>(pipelines).FirstOrDefault();
        if (document == null)
        {
            throw new InvalidOperationException("No matching document found.");
        }

        return (document["ChampionshipId"].ToInt32(), document["Round"].ToInt32());
    }

    public string[] GetSheetNames(string network)
    {
        var collection = GetCollection<BsonDocument>(network);

        var projection = Builders<BsonDocument>.Projection.Include("State.Name").Exclude("_id");
        var documents = collection.Find(new BsonDocument()).Project(projection).ToList();

        List<string> names = new List<string>();
        foreach (var document in documents)
        {
            if (document.Contains("State") && document["State"].AsBsonDocument.Contains("Name"))
            {
                names.Add(document["State"]["Name"].AsString);
            }
        }

        return names.ToArray();
    }

    public async Task<string> GetSheet(string network, string sheetName, SheetFormat sheetFormat)
    {
        var collection = GetCollection<BsonDocument>(network);
        var gridFs = new GridFSBucket(GetDatabase(network));

        string fieldToInclude = sheetFormat switch
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
            SheetFormat.Csv
                => await RetrieveFromGridFs(gridFs, document[fieldToInclude].AsObjectId),
            _
                => document[fieldToInclude]
                    .ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict })
        };
    }

    private async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }
}
