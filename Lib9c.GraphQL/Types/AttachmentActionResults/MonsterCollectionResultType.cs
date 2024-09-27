using HotChocolate.Types;
using Lib9c.Models.AttachmentActionResults;

namespace Lib9c.GraphQL.Types.AttachmentActionResults;

public class MonsterCollectionResultType : ObjectType<MonsterCollectionResult>
{
    protected override void Configure(IObjectTypeDescriptor<MonsterCollectionResult> descriptor)
    {
        descriptor.Implements<AttachmentActionResultInterfaceType>();
    }
}
