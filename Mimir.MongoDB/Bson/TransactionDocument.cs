using Lib9c.Models.Block;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using HotChocolate;
using Libplanet.Crypto;

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
    Address? firstAvatarAddressInActionArguments,
    string? firstNCGAmountInActionArguments,
    Address? firstRecipientInActionArguments,
    Address? firstSenderInActionArguments,
    Transaction Object
) : MimirBsonDocument(TxId, new DocumentMetadata(2, StoredBlockIndex));
