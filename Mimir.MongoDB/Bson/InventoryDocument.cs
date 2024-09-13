using Lib9c.Models.Items;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record InventoryDocument(Address Address, Inventory Object) : MimirBsonDocument(Address);
