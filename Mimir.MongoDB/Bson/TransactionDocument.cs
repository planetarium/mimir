using Lib9c.Models.Block;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using HotChocolate;

namespace Mimir.MongoDB.Bson;

/// <summary>
/// Not MongoDB collection, it is a NineChronicles' collection state.
/// </summary>
/// <param name="Object"></param>
[BsonIgnoreExtraElements]
public record TransactionDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] string TxId,
    string BlockHash,
    long BlockIndex,
    string firstActionTypeId,
    string? firstAvatarAddressInActionArguments,
    string? firstNCGAmountInActionArguments,
    Transaction Object
) : MimirBsonDocument(TxId, new DocumentMetadata(1, StoredBlockIndex));
