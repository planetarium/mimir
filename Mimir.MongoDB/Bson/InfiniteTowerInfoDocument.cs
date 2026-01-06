using HotChocolate;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record InfiniteTowerInfoDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] Address Address,
    Address AvatarAddress,
    int InfiniteTowerId,
    InfiniteTowerInfo Object
) : MimirBsonDocument($"{AvatarAddress.ToHex()}_{InfiniteTowerId}", new DocumentMetadata(1, StoredBlockIndex));





