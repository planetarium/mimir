using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class WorldInformationStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        var state = new WorldInformationState(context.RawState);
        var lastStageClearedId = state.WorldDictionary
            .Where(kvp => kvp.Key != 10001)
            .Select(kvp => kvp.Value.StageClearedId)
            .Where(id => id >= 0)
            .DefaultIfEmpty(0)
            .Max();

        return new WorldInformationDocument(
            context.BlockIndex,
            context.Address,
            lastStageClearedId,
            state
        );
    }
}