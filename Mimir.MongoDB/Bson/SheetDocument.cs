using Bencodex.Types;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record SheetDocument(
    Address Address,
    ISheet Object,
    string Name,
    IValue RawState)
    : MimirBsonDocument(Address);
