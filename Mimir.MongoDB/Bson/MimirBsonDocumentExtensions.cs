using Mimir.MongoDB.Json.Converters;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mimir.MongoDB.Bson;

public static class MimirBsonDocumentExtensions
{
    public static WriteModel<BsonDocument> ToUpdateOneModel(this MimirBsonDocument document)
    {
        var json = document.ToJson();
        var bsonDocument = BsonDocument.Parse(json);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", document.Address.ToHex());
        var update = new BsonDocument("$set", bsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        return upsertOne;
    }

    public static readonly JsonSerializerSettings JsonSerializerSettings =
        new()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters =
            [
                new BigIntegerJsonConverter(),
                new MaterialAndIntDictionaryJsonConverter(),
                new StringEnumConverter(),
            ],
            ContractResolver = new IgnoreBencodedContractResolver(),
        };

    public static string ToJson(this MimirBsonDocument document) =>
        JsonConvert.SerializeObject(document, JsonSerializerSettings);
}
