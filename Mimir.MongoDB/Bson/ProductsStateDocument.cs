using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ProductsStateDocument(Address Address, ProductsState Object, Address AvatarAddress)
    : MimirBsonDocument(Address) { }
