using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

/// <summary>
/// Not MongoDB collection, it is a NineChronicles' collection state.
/// </summary>
/// <param name="Object"></param>
[BsonIgnoreExtraElements]
public record CollectionDocument(Address Address, CollectionState Object) : MimirBsonDocument(Address);
