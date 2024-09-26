using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class ItemEnhancement7ResultType : ObjectType<ItemEnhancement7Result>
{
    protected override void Configure(IObjectTypeDescriptor<ItemEnhancement7Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
