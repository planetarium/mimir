using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;

public class AddressSerializer : StructSerializerBase<Address>
{
    public static readonly AddressSerializer Instance = new();

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        Address value) =>
        context.Writer.WriteString(value.ToHex());

    public override Address Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        var bsonType = reader.GetCurrentBsonType();
        return bsonType is BsonType.String
            ? new Address(reader.ReadString())
            : throw new UnexpectedTypeOfBsonValueException([BsonType.String], bsonType);
    }
}
