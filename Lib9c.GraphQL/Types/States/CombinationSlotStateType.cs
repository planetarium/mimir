using HotChocolate.Types;
using Lib9c.GraphQL.Types.AttachmentActionResults;
using Lib9c.Models.States;

namespace Lib9c.GraphQL.Types.States;

public class CombinationSlotStateType : ObjectType<CombinationSlotState>
{
    protected override void Configure(IObjectTypeDescriptor<CombinationSlotState> descriptor)
    {
        descriptor
            .Field(f => f.Result)
            .Type<AttachmentActionResultInterfaceType>();
    }
}
