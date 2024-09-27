using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class DailyReward2ResultType : ObjectType<DailyReward2Result>
{
    protected override void Configure(IObjectTypeDescriptor<DailyReward2Result> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
