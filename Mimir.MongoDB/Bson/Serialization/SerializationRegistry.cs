using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace Mimir.MongoDB.Bson.Serialization;

public static class SerializationRegistry
{
    public static void Register()
    {
        BsonSerializer.RegisterSerializer(typeof(Address), new LibplanetCryptoAddressBsonSerializer());
    }
}
