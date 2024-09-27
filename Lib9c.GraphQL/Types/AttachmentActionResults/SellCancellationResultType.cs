using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class SellCancellationResultType : ObjectType<SellCancellationResult>
{
    protected override void Configure(IObjectTypeDescriptor<SellCancellationResult> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
