using Lib9c.Models.Market;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ProductsStateDocument(
    long StoredBlockIndex,
    Address Address,
    ProductsState Object,
    Address AvatarAddress
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
