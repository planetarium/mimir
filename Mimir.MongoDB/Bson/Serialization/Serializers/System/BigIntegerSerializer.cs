using System.Numerics;
using Mimir.MongoDB.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.System;

public class BigIntegerSerializer : StructSerializerBase<BigInteger>
{
    public static readonly BigIntegerSerializer Instance = new();

    public override BigInteger Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        var bsonType = reader.GetCurrentBsonType();
        return bsonType is BsonType.String
            ? BigInteger.Parse(reader.ReadString())
            : throw new UnexpectedTypeOfBsonValueException([BsonType.String], bsonType);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(
    //     BsonSerializationContext context,
    //     BsonSerializationArgs args,
    //     BigInteger value) =>
    //     context.Writer.WriteString(value.ToString());
}
