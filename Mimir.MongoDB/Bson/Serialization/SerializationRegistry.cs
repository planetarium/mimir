using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using MongoDB.Bson.Serialization;

namespace Mimir.MongoDB.Bson.Serialization;

public static class SerializationRegistry
{
    public static void Register()
    {
        // Libplanet
        BsonSerializer.RegisterSerializer(typeof(Address), new AddressSerializer());
    }
}
