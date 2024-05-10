using Mimir.Models.Avatar;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;

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

    public string GetSheet(string network, string sheetName)
    {
        var collection = GetCollection(network);

        var projection = Builders<BsonDocument>.Projection.Include("Sheet").Exclude("_id");

        var filter = Builders<BsonDocument>.Filter.Eq("Name", sheetName);
        var document = collection.Find(filter).Project(projection).FirstOrDefault();

        if (document == null)
        {
            return "{}";
        }

        return document["Sheet"].ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
    }
}
