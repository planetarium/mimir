using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;

public class RapidCombination0ResultSerializer : ClassSerializerBase<RapidCombination0Result>
{
    public static readonly RapidCombination0ResultSerializer Instance = new();

    private static readonly IBsonSerializer<Dictionary<Material, int>> MaterialsSerializer =
        new DictionaryInterfaceImplementerSerializer<Dictionary<Material, int>>()
            .WithKeySerializer(MaterialSerializer.Instance);

    public static RapidCombination0Result Deserialize(BsonDocument doc) => new()
    {
        TypeId = doc["TypeId"].AsString,
        ItemUsable = doc.TryGetValue("ItemUsable", out var itemUsableBsonValue)
            ? ItemUsableSerializer.Deserialize(itemUsableBsonValue.AsBsonDocument)
            : null,
        Costume = doc.TryGetValue("Costume", out var costumeBsonValue)
            ? CostumeSerializer.Deserialize(costumeBsonValue.AsBsonDocument)
            : null,
        TradableFungibleItem = doc.TryGetValue("TradableFungibleItem", out var tradableFungibleItemBsonValue)
            ? TradableMaterialSerializer.Deserialize(tradableFungibleItemBsonValue.AsBsonDocument)
            : null,
        TradableFungibleItemCount = doc["TradableFungibleItemCount"].AsInt32,
        Cost = BsonSerializer.Deserialize<Dictionary<Material, int>>(doc["Cost"].AsBsonDocument),
    };

    public override RapidCombination0Result Deserialize(BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, RapidCombination0Result value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
