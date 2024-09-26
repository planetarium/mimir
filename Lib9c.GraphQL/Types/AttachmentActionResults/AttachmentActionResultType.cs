using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class AttachmentActionResultType : ObjectType<AttachmentActionResult>
{
    protected override void Configure(IObjectTypeDescriptor<AttachmentActionResult> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
