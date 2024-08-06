using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson;

public record SheetDocument(
    Address Address,
    ISheet Object,
    string Name,
    IValue RawState
) : MimirBsonDocument(Address);
