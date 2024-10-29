using System.Security.Cryptography;
using Lib9c.Models.Items;
using Libplanet.Common;
using Mimir.MongoDB.Bson.Extensions;
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

        if (doc.Contains("RequiredBlockIndex"))
        {
            return TradableMaterialSerializer.Deserialize(doc);
        }

        return new Material
        {
            Id = doc["Id"].AsInt32,
            Grade = doc["Grade"].AsInt32,
            ItemType = itemType,
            ItemSubType = itemSubType,
            ElementalType = doc["ElementalType"].ToEnum<Nekoyume.Model.Elemental.ElementalType>(),
            ItemId = HashDigest<SHA256>.FromString(doc["ItemId"].AsString),
        };
    }

    public static Material Deserialize(string jsonString)
    {
        var material = JsonConvert.DeserializeObject<Material>(
            jsonString,
            MimirBsonDocumentExtensions.JsonSerializerSettings);  // Use JsonSerializerOptions from System.Text.Json

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
