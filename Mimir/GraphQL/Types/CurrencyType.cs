using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class CurrencyType : ObjectType<Currency>
{
    protected override void Configure(IObjectTypeDescriptor<Currency> descriptor)
    {
        descriptor.Field(t => t.Ticker).Type<StringType>();
        descriptor.Field(t => t.DecimalPlaces).Type<IntType>();
        descriptor.Field(t => t.Hash).Type<StringType>();
        descriptor.Field(t => t.TotalSupplyTrackable).Type<BooleanType>();
    }
}
