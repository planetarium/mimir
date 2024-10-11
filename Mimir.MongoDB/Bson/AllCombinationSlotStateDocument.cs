using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record AllCombinationSlotStateDocument(Address Address, AllCombinationSlotState Object) :
    MimirBsonDocument(Address);
