using Bencodex.Types;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record SheetDocument(
    long StoredBlockIndex,
    Address Address,
    ISheet Object,
    string Name,
    IValue RawState
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
