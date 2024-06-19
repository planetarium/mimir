using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class ProductStateType : ObjectType<ProductState>
{
    protected override void Configure(IObjectTypeDescriptor<ProductState> descriptor)
    {
        descriptor.Field(s => s.AvatarAddress).Type<NonNullType<StringType>>();
        descriptor.Field(s => s.Object).Type<NonNullType<ProductObjectUnionType>>();
        descriptor.Field(p => p.CombatPoint).Type<IntType>();
        descriptor.Field(p => p.UnitPrice).Type<IntType>();
        descriptor.Field(p => p.Crystal).Type<IntType>();
        descriptor.Field(p => p.CrystalPerPrice).Type<IntType>();
        descriptor.Field(p => p.Raw).Type<NonNullType<StringType>>();
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
