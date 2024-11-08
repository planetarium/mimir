using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class AllRuneStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        var allRuneState = new AllRuneState(context.RawState);
        return new AllRuneDocument(context.BlockIndex, context.Address, allRuneState);
    }
}