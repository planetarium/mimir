using HotChocolate.Types;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.InputObjects;

namespace Lib9c.GraphQL.Types;

public class CurrencyInputType : InputObjectType<CurrencyInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<CurrencyInput> descriptor)
    {
        descriptor
            .Field(f => f.CurrencyMethodType)
            .Type<NonNullType<EnumType<CurrencyMethodType>>>();
        descriptor
            .Field(f => f.Ticker)
            .Type<NonNullType<StringType>>();
        descriptor
            .Field(f => f.DecimalPlaces)
            .Type<NonNullType<ByteType>>();
        descriptor
            .Field(f => f.Minters)
            .Type<ListType<AddressType>>();
        descriptor
            .Field(f => f.TotalSupplyTrackable)
            .Type<NonNullType<BooleanType>>()
            .DefaultValue(false);
        descriptor
            .Field(f => f.MaximumSupplyMajor)
            .Type<LongType>();
        descriptor
            .Field(f => f.MaximumSupplyMinor)
            .Type<LongType>();
    }
}
