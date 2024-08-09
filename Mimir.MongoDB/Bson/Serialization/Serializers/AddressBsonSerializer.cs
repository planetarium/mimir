using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers;

public class AddressBsonSerializer : SerializerBase<Address>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Address value) =>
        context.Writer.WriteString(value.ToHex());

    public override Address Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        return bsonType is BsonType.String
            ? new Address(context.Reader.ReadString())
            : throw new UnexpectedTypeOfBsonValueException(
                [BsonType.String],
                bsonType);
    }
}
