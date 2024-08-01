using Libplanet.Crypto;
using Nekoyume.Model.Item;

namespace Mimir.MongoDB.Bson;

public record InventoryDocument(Address Address, Inventory Object) : IMimirBsonDocument(Address) { }
