using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.Address).Type<StringType>();
        descriptor.Field(p => p.State).Type<ProductStateType>();
        descriptor.Field(p => p.CombatPoint).Type<IntType>();
        descriptor.Field(p => p.UnitPrice).Type<IntType>();
        descriptor.Field(p => p.Crystal).Type<IntType>();
        descriptor.Field(p => p.CrystalPerPrice).Type<IntType>();
        descriptor.Field(p => p.Raw).Type<StringType>();
    }
}
