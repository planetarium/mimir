using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class ItemEnhancement12ResultType : ObjectType<ItemEnhancement12Result>
{
    protected override void Configure(IObjectTypeDescriptor<ItemEnhancement12Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
