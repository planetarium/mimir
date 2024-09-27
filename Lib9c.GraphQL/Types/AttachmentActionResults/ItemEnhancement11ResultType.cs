using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class ItemEnhancement11ResultType : ObjectType<ItemEnhancement11Result>
{
    protected override void Configure(IObjectTypeDescriptor<ItemEnhancement11Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
