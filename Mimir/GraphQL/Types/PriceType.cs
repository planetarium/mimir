using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class PriceType : ObjectType<Price>
{
    protected override void Configure(IObjectTypeDescriptor<Price> descriptor)
    {
        descriptor.Field(t => t.Currency).Type<CurrencyType>();
        descriptor.Field(t => t.RawValue).Type<StringType>();
        descriptor.Field(t => t.Sign).Type<IntType>();
        descriptor.Field(t => t.MajorUnit).Type<StringType>();
        descriptor.Field(t => t.MinorUnit).Type<StringType>();
    }
}
