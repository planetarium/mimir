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
        return "tableSheets";
    }

    public string[] GetSheetNames(string network)
    {
        var collection = GetCollection(network);

        var projection = Builders<BsonDocument>.Projection.Include("Name").Exclude("_id");
        var documents = collection.Find(new BsonDocument()).Project(projection).ToList();

        List<string> names = new List<string>();
        foreach (var document in documents)
        {
            if (document.Contains("Name"))
            {
                names.Add(document["Name"].AsString);
            }
        }

        return names.ToArray();
    }

    public async Task<string> GetSheet(string network, string sheetName, SheetFormat sheetFormat)
    {
        var collection = GetCollection(network);
        var gridFs = new GridFSBucket(GetDatabase(network));

        string fieldToInclude = sheetFormat switch
        {
            SheetFormat.Csv => "SheetCsvFileId",
            SheetFormat.Json => "SheetJson",
            _ => "SheetJson"
        };

        var projection = Builders<BsonDocument>.Projection.Include(fieldToInclude).Exclude("_id");
        var filter = Builders<BsonDocument>.Filter.Eq("Name", sheetName);
        var document = collection.Find(filter).Project(projection).FirstOrDefault();

        if (
            document == null
            || !document.Contains(fieldToInclude)
            || document[fieldToInclude].IsBsonNull
        )
        {
            return sheetFormat == SheetFormat.Json ? "{}" : "";
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
