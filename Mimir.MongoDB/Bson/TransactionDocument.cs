using HotChocolate;
using Lib9c.Models.Block;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

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
    ExtractedActionValues extractedActionValues,
    Transaction Object
) : MimirBsonDocument(TxId, new DocumentMetadata(3, StoredBlockIndex));

/// <summary>
/// Extracted action values from the action arguments.
/// </summary>
/// <param name="ActionTypeId"></param>
/// <param name="NCGAmount"></param>
/// <param name="AvatarAddress"></param>
/// <param name="Recipient"></param>
/// <param name="Sender"></param>
public record ExtractedActionValues(
    string TypeId,
    Address? AvatarAddress,
    Address? Sender,
    List<RecipientInfo>? Recipients,
    List<string>? FungibleAssetValues,
    List<Address>? InvolvedAvatarAddresses,
    List<Address>? InvolvedAddresses
);

/// <summary>
/// Recipient information for asset transfers.
/// </summary>
public record RecipientInfo(Address Recipient, string Amount);
