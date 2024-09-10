using Lib9c.Models.Items;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record InventoryDocument(Address Address, Inventory Object) : MimirBsonDocument(Address) { }
