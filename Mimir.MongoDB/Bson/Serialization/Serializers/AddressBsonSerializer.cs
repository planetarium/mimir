using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers;

public class AddressBsonSerializer : SerializerBase<Address>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Address value) =>
        context.Writer.WriteString(value.ToHex());

    public override Address Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
        context.Reader.GetCurrentBsonType() is BsonType.String
            ? new Address(context.Reader.ReadString())
            : throw new Exception();
}
