using HotChocolate.Types;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Types;

public class CurrencyType : ObjectType<Currency>
{
    protected override void Configure(IObjectTypeDescriptor<Currency> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor
            .Field("ticker")
            .Type<NonNullType<StringType>>()
            .Resolve(context => context.Parent<Currency>().Ticker);
        descriptor
            .Field("decimalPlaces")
            .Type<NonNullType<ByteType>>()
            .Resolve(context => context.Parent<Currency>().DecimalPlaces);
        descriptor
            .Field("minters")
            .Type<ListType<AddressType>>()
            .Resolve(context => context.Parent<Currency>().Minters);
        descriptor
            .Field("totalSupplyTrackable")
            .Type<NonNullType<BooleanType>>()
            .Resolve(context => context.Parent<Currency>().TotalSupplyTrackable);
    }
}
