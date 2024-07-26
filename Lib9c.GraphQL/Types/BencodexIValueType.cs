using Bencodex.Types;
using HotChocolate.Types;

namespace Mimir.GraphQL.Types;

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
