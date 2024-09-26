using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class RapidCombination5ResultType : ObjectType<RapidCombination5Result>
{
    protected override void Configure(IObjectTypeDescriptor<RapidCombination5Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
