using Bencodex.Types;
using HotChocolate;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.TableData;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record SheetDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] Address Address,
    ISheet Object,
    string Name,
    IValue RawState
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex));
