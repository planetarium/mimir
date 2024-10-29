using Mimir.MongoDB.Exceptions;
using MongoDB.Bson;

namespace Mimir.MongoDB.Bson.Extensions;

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

    public static T ToEnum<T>(this BsonValue value) where T : struct =>
        value.BsonType switch
        {
            BsonType.Int32 => (T)Enum.ToObject(typeof(T), value.AsInt32),
            BsonType.Int64 => (T)Enum.ToObject(typeof(T), value.AsInt64),
            BsonType.String => Enum.Parse<T>(value.AsString),
            _ => throw new UnexpectedTypeOfBsonValueException(
                [BsonType.Int32, BsonType.Int64, BsonType.String],
                value.BsonType)
        };
}
