using Lib9c.Models.AttachmentActionResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;

public class AttachmentActionResultSerializer : ClassSerializerBase<AttachmentActionResult>
{
    public static readonly AttachmentActionResultSerializer Instance = new();

    public static AttachmentActionResult Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("TypeId", out var typeIdBsonValue))
        {
            throw new BsonSerializationException("Missing TypeId in document.");
        }

        var typeId = typeIdBsonValue.AsString;
        return typeId switch
        {
            "buy.buyerResult" => Buy7BuyerResultSerializer.Deserialize(doc),
            "buy.sellerResult" => Buy7SellerResultSerializer.Deserialize(doc),
            "combination.result-model" => CombinationConsumable5ResultSerializer.Deserialize(doc),
            "dailyReward.dailyRewardResult" => DailyReward2ResultSerializer.Deserialize(doc),
            "itemEnhancement.result" => ItemEnhancement7ResultSerializer.Deserialize(doc),
            "item_enhancement9.result" => ItemEnhancement9ResultSerializer.Deserialize(doc),
            "item_enhancement11.result" => ItemEnhancement11ResultSerializer.Deserialize(doc),
            "item_enhancement12.result" => ItemEnhancement12ResultSerializer.Deserialize(doc),
            "item_enhancement13.result" => ItemEnhancement13ResultSerializer.Deserialize(doc),
            "monsterCollection.result" => MonsterCollectionResultSerializer.Deserialize(doc),
            "rapidCombination.result" => RapidCombination0ResultSerializer.Deserialize(doc),
            "rapid_combination5.result" => RapidCombination5ResultSerializer.Deserialize(doc),
            "sellCancellation.result" => SellCancellationResultSerializer.Deserialize(doc),
            _ => throw new BsonSerializationException($"Unsupported TypeId: {typeId}"),
        };
    }

    public override AttachmentActionResult Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, AttachmentActionResult value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
