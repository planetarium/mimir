using MongoDB.Bson;

namespace Mimir.MongoDB.Bson.Extensions;

public static class BsonDocumentExtensions
{
    public static T? GetValueOrDefault<T>(this BsonDocument document, string name, Func<BsonValue, T> converter)
    {
        T? value = default;
        if (document.TryGetValue(name, out var bsonValue))
        {
            value = converter(bsonValue);
        }

        return value;
    }
}