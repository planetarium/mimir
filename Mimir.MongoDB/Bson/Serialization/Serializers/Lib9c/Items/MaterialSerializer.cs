using System.Security.Cryptography;
using Lib9c.Models.Items;
using Libplanet.Common;
using Mimir.MongoDB.Json.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class MaterialSerializer : ClassSerializerBase<Material>
{
    public static readonly MaterialSerializer Instance = new();

    public static Material Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("ItemType", out var itemTypeValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        if (!doc.TryGetValue("ItemSubType", out var itemSubTypeValue))
        {
            throw new BsonSerializationException("Missing itemSubTypeValue in document.");
        }

        var itemType = (Nekoyume.Model.Item.ItemType)itemTypeValue.AsInt32;
        var itemSubType = (Nekoyume.Model.Item.ItemSubType)itemSubTypeValue.AsInt32;
        if (itemType != Nekoyume.Model.Item.ItemType.Material)
        {
            throw new BsonSerializationException($"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}");
        }

        if (doc.Contains("RequiredBlockIndex"))
        {
            return TradableMaterialSerializer.Deserialize(doc);
        }

        return new Material
        {
            Id = doc["Id"].AsInt32,
            Grade = doc["Grade"].AsInt32,
            ItemType = (Nekoyume.Model.Item.ItemType)doc["ItemType"].AsInt32,
            ItemSubType = (Nekoyume.Model.Item.ItemSubType)doc["ItemSubType"].AsInt32,
            ElementalType = (Nekoyume.Model.Elemental.ElementalType)doc["ElementalType"].AsInt32,
            ItemId = HashDigest<SHA256>.FromString(doc["ItemId"].AsString),
        };
    }

    public static Material Deserialize(string jsonString)
    {
        var material = JsonConvert.DeserializeObject<Material>(
            jsonString,
            MimirBsonDocumentExtensions.JsonSerializerSettings);
        if (material is null)
        {
            throw new BsonSerializationException("Failed to deserialize Material from JSON string.");
        }

        return material;
    }

    public override Material Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
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
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Material value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
