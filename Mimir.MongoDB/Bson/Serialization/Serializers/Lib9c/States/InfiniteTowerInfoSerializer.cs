using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.States;

public class InfiniteTowerInfoSerializer : ClassSerializerBase<InfiniteTowerInfo>
{
    public static readonly InfiniteTowerInfoSerializer Instance = new();

    public static InfiniteTowerInfo Deserialize(BsonDocument doc) => new()
    {
        Address = new Address(doc["Address"].AsString),
        InfiniteTowerId = doc["InfiniteTowerId"].AsInt32,
        ClearedFloor = doc["ClearedFloor"].AsInt32,
        RemainingTickets = doc["RemainingTickets"].AsInt32,
        TotalTicketsUsed = doc["TotalTicketsUsed"].AsInt32,
        NumberOfTicketPurchases = doc["NumberOfTicketPurchases"].AsInt32,
        LastResetBlockIndex = doc["LastResetBlockIndex"].ToLong(),
        LastTicketRefillBlockIndex = doc["LastTicketRefillBlockIndex"].ToLong(),
    };

    public override InfiniteTowerInfo Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, InfiniteTowerInfo value)
    // {
    //     base.Serialize(context, args, value);
    // }
}





