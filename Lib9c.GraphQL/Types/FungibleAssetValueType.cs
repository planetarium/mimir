using HotChocolate.Types;
using Libplanet.Types.Assets;

namespace Lib9c.GraphQL.Types;

public class FungibleAssetValueType : ObjectType<FungibleAssetValue>
{
    protected override void Configure(IObjectTypeDescriptor<FungibleAssetValue> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor
            .Field("rawValue")
            .Type<NonNullType<StringType>>()
            .Resolve(context => context.Parent<FungibleAssetValue>().RawValue.ToString());

        descriptor
            .Field("quantity")
            .Type<NonNullType<StringType>>()
            .Resolve(context => context.Parent<FungibleAssetValue>().GetQuantityString());

        descriptor
            .Field("ticker")
            .Type<NonNullType<StringType>>()
            .Resolve(context => context.Parent<FungibleAssetValue>().Currency.Ticker);

        descriptor
            .Field("decimalPlaces")
            .Type<NonNullType<ByteType>>()
            .Resolve(context => context.Parent<FungibleAssetValue>().Currency.DecimalPlaces);
    }
}
