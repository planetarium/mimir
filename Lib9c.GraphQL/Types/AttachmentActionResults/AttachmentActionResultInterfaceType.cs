using HotChocolate.Types;
using Lib9c.GraphQL.Types.Items;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class AttachmentActionResultInterfaceType : InterfaceType<AttachmentActionResult>
{
    protected override void Configure(IInterfaceTypeDescriptor<AttachmentActionResult> descriptor)
    {
        descriptor.Name("AttachmentActionResultInterface");
        descriptor
            .Field(f => f.ItemUsable)
            .Type<ItemUsableInterfaceType>();
    }
}
