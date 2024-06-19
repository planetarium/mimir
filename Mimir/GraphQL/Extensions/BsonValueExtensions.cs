using Mimir.Exceptions;
using MongoDB.Bson;

namespace Mimir.GraphQL.Extensions;

public static class BsonValueExtensions
{
    public static long ToLong(this BsonValue value) =>
        value.BsonType switch
        {
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            _ => throw new UnexpectedTypeOfBsonValueException(
                [BsonType.Int32, BsonType.Int64],
                value.BsonType)
        };
}
