using Bencodex.Types;
using HotChocolate.Types;

namespace Lib9c.GraphQL.Types;

public class BencodexIValueType : ObjectType<IValue>
{
    protected override void Configure(IObjectTypeDescriptor<IValue> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.Inspect())
            .Type<StringType>();
    }
}
