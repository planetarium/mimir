using Lib9c.Models.States;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.AttachmentActionResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.States;

public class CombinationSlotStateSerializer : ClassSerializerBase<CombinationSlotState>
{
    public static readonly CombinationSlotStateSerializer Instance = new();

    public static CombinationSlotState Deserialize(BsonDocument doc) => new()
    {
        UnlockBlockIndex = doc["UnlockBlockIndex"].ToLong(),
        UnlockStage = doc["UnlockStage"].AsInt32,
        StartBlockIndex = doc["StartBlockIndex"].ToLong(),
        Result = doc.TryGetValue("Result", out var resultBsonValue)
            ? AttachmentActionResultSerializer.Deserialize(resultBsonValue.AsBsonDocument)
            : null,
        PetId = doc.TryGetValue("PetId", out var petIdBsonValue)
            ? petIdBsonValue.AsInt32
            : null,
    };

    public override CombinationSlotState Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, CombinationSlotState value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
