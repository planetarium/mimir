using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class ItemEnhancement13ResultType : ObjectType<ItemEnhancement13Result>
{
    protected override void Configure(IObjectTypeDescriptor<ItemEnhancement13Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
