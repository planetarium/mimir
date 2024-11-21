using Bencodex.Types;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.TableData;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record SheetDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    ISheet Object,
    string Name,
    IValue RawState
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex));
