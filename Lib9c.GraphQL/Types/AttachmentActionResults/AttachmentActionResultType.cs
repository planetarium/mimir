using HotChocolate.Types;
using Lib9c.GraphQL.Types.Items;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class AttachmentActionResultType : ObjectType<AttachmentActionResult>
{
    protected override void Configure(IObjectTypeDescriptor<AttachmentActionResult> descriptor)
    {
        descriptor
            .Field(f => f.ItemUsable)
            .Type<ItemUsableInterfaceType>();
    }
}
