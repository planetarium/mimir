using System.Numerics;
using Lib9c.Models.AttachmentActionResults;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;
using Mimir.MongoDB.Bson.Serialization.Serializers.Libplanet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;

public class ItemEnhancement13ResultSerializer : ClassSerializerBase<ItemEnhancement13Result>
{
    public static readonly ItemEnhancement13ResultSerializer Instance = new();

    public static ItemEnhancement13Result Deserialize(BsonDocument doc) => new()
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
        Id = Guid.Parse(doc["Id"].AsString),
        MaterialItemIdList = doc["MaterialItemIdList"].AsBsonArray.Select(id => Guid.Parse(id.AsString)),
        Gold = BigInteger.Parse(doc["Gold"].AsString),
        ActionPoint = doc["ActionPoint"].AsInt32,
        EnhancementResult = doc["EnhancementResult"].ToEnum<Nekoyume.Action.ItemEnhancement9.EnhancementResult>(),
        PreItemUsable = doc.TryGetValue("PreItemUsable", out var preItemUsableBsonValue)
            ? ItemUsableSerializer.Deserialize(preItemUsableBsonValue.AsBsonDocument)
            : null,
        Crystal = FungibleAssetValueSerializer.Deserialize(doc["Crystal"].AsBsonDocument),
    };

    public override ItemEnhancement13Result Deserialize(BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ItemEnhancement13Result value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
