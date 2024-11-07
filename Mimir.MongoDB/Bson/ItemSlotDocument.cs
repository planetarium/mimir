using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ItemSlotDocument(long StoredBlockIndex, Address Address, ItemSlotState Object)
    : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
