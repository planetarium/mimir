using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class AssetType : ObjectType<Asset>
{
    protected override void Configure(IObjectTypeDescriptor<Asset> descriptor)
    {
        descriptor.Field(a => a.Currency).Type<CurrencyType>();
        descriptor.Field(a => a.RawValue).Type<StringType>();
        descriptor.Field(a => a.Sign).Type<IntType>();
        descriptor.Field(a => a.MajorUnit).Type<StringType>();
        descriptor.Field(a => a.MinorUnit).Type<StringType>();
    }
}
