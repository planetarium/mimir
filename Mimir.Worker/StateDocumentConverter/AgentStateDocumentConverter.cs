using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class AgentStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not List l)
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(context.RawState),
                new[] { ValueKind.List },
                context.RawState.Kind
            );

        var agentState = new AgentState(l);
        return new AgentDocument(context.BlockIndex, context.Address, agentState);
    }
}
