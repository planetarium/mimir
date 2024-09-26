using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class ItemEnhancement9ResultType : ObjectType<ItemEnhancement9Result>
{
    protected override void Configure(IObjectTypeDescriptor<ItemEnhancement9Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
