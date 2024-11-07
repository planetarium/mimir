using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldInformationDocument(
    long StoredBlockIndex,
    Address Address,
    WorldInformationState Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
