using Bencodex.Types;
using HotChocolate.Types;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.GraphQL.Types;

public class BencodexIValueType : ObjectType<IValue>
{
    protected override void Configure(IObjectTypeDescriptor<IValue> descriptor)
    {
        descriptor
            .Field(f => f.Kind)
            .Type<NonNullType<EnumType<ValueKind>>>();
    }
}
