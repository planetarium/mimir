using Mimir.GraphQL.Objects;

namespace Mimir.GraphQL.Types;

public class RuneType : ObjectType<RuneObject>
{
    protected override void Configure(IObjectTypeDescriptor<RuneObject> descriptor)
    {
        descriptor
            .Field(f => f.RuneSheetId)
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.Level)
            .Type<NonNullType<IntType>>();
    }
}
