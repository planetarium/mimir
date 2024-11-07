using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record AllCombinationSlotStateDocument(
    long StoredBlockIndex,
    Address Address,
    AllCombinationSlotState Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
