using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record CombinationSlotStateDocument(Address Address, CombinationSlotState Object) :
    MimirBsonDocument(Address);
