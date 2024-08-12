using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using Mimir.MongoDB.Bson.Serialization.Serializers.System;
using MongoDB.Bson.Serialization;

namespace Mimir.MongoDB.Bson.Serialization;

public static class SerializationRegistry
{
    public static void Register()
    {
        // System
        BsonSerializer.RegisterSerializer(typeof(BigInteger), BigIntegerSerializer.Instance);

        // Libplanet
        BsonSerializer.RegisterSerializer(typeof(Address), AddressSerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(Currency), CurrencySerializer.Instance);
        BsonSerializer.RegisterSerializer(typeof(FungibleAssetValue), FungibleAssetValueSerializer.Instance);
    }
}
