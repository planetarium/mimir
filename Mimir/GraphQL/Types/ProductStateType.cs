using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class ProductStateType : ObjectType<ProductState>
{
    protected override void Configure(IObjectTypeDescriptor<ProductState> descriptor)
    {
        descriptor.Field(s => s.AvatarAddress).Type<StringType>();
        descriptor.Field(s => s.ProductsStateAddress).Type<StringType>();
        descriptor.Field(s => s.Object).Type<ProductObjectUnionType>();
    }
}

public class ProductObjectUnionType : UnionType<IProductObject>
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        descriptor.Type<ItemProductObjectType>();
        descriptor.Type<FavProductObjectType>();
    }
}
