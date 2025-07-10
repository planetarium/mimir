using HotChocolate;
using System.Numerics;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record StakeDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] Address Address,
    Address AgentAddress,
    StakeState? Object,
    BigInteger Amount
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(2, StoredBlockIndex));
