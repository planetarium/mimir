using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class Buy7BuyerResultType : ObjectType<Buy7BuyerResult>
{
    protected override void Configure(IObjectTypeDescriptor<Buy7BuyerResult> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
