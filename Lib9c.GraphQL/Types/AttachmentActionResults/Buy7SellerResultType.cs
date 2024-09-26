using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class Buy7SellerResultType : ObjectType<Buy7SellerResult>
{
    protected override void Configure(IObjectTypeDescriptor<Buy7SellerResult> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
