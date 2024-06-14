using Lib9c.GraphQL.Types;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;

namespace Mimir.GraphQL.Types;

public class InventoryType : ObjectType<InventoryObject>
{
    protected override void Configure(IObjectTypeDescriptor<InventoryObject> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field("consumables")
            .Description("The consumables in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .ResolveWith<InventoryResolver>(_ =>
                InventoryResolver.GetConsumables(default!, default!, default!, default!, default!));
        descriptor
            .Field("costumes")
            .Description("The costumes in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .ResolveWith<InventoryResolver>(_ =>
                InventoryResolver.GetCostumes(default!, default!, default!, default!, default!));
        descriptor
            .Field("equipments")
            .Description("The equipments in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .ResolveWith<InventoryResolver>(_ =>
                InventoryResolver.GetEquipments(default!, default!, default!, default!, default!));
        descriptor
            .Field("materials")
            .Description("The materials in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .ResolveWith<InventoryResolver>(_ =>
                InventoryResolver.GetMaterials(default!, default!, default!, default!, default!));
    }
}
