using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class RapidCombination0ResultType : ObjectType<RapidCombination0Result>
{
    protected override void Configure(IObjectTypeDescriptor<RapidCombination0Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
