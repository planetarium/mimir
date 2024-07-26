using Bencodex.Types;
using Nekoyume.TableData;

namespace Mimir.MongoDB.Bson;

public record SheetDocument(
    ISheet Object,
    string Name,
    IValue RawState,
    IValue Bencoded)
    : IMimirBsonDocument;
