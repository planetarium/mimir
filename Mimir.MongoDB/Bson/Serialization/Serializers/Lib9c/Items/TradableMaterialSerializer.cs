using System.Security.Cryptography;
using Lib9c.Models.Items;
using Libplanet.Common;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class TradableMaterialSerializer : ClassSerializerBase<TradableMaterial>
{
    public static readonly TradableMaterialSerializer Instance = new();

    public static TradableMaterial Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("ItemType", out var itemTypeBsonValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        if (!doc.TryGetValue("ItemSubType", out var itemSubTypeBsonValue))
        {
            throw new BsonSerializationException("Missing itemSubTypeValue in document.");
        }

        var itemType = itemTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemType>();
        var itemSubType = itemSubTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemSubType>();
        if (itemType != Nekoyume.Model.Item.ItemType.Material)
        {
            throw new BsonSerializationException($"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}");
        }

        return new TradableMaterial
        {
            Id = doc["Id"].AsInt32,
            Grade = doc["Grade"].AsInt32,
            ItemType = itemType,
            ItemSubType = itemSubType,
            ElementalType = doc["ElementalType"].ToEnum<Nekoyume.Model.Elemental.ElementalType>(),
            ItemId = HashDigest<SHA256>.FromString(doc["ItemId"].AsString),
            RequiredBlockIndex = doc["RequiredBlockIndex"].ToLong(),
        };
    }

    public static TradableMaterial Deserialize(string jsonString)
    {
        var material = JsonConvert.DeserializeObject<TradableMaterial>(
            jsonString,
            MimirBsonDocumentExtensions.JsonSerializerSettings);
        if (material is null)
        {
            throw new BsonSerializationException("Failed to deserialize Material from JSON string.");
        }

        return material;
    }

    public override TradableMaterial Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        var bsonType = reader.GetCurrentBsonType();
        switch (bsonType)
        {
            case BsonType.Document:
                var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
                return Deserialize(doc);
            case BsonType.String:
                var jsonString = reader.ReadString();
                return Deserialize(jsonString);
            default:
                throw new BsonSerializationException($"Cannot deserialize Material from BsonType {bsonType}.");
        }
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TradableMaterial value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
