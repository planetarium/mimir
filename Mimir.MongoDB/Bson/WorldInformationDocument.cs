using HotChocolate;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldInformationDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] Address Address,
    int LastStageClearedId,
    WorldInformationState Object
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(2, StoredBlockIndex));
