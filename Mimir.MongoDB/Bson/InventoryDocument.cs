using Lib9c.Models.Items;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record InventoryDocument(long StoredBlockIndex, Address Address, Inventory Object)
    : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
