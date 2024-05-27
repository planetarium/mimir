using HotChocolate.Types;
using Lib9c.GraphQL.Objects;

namespace Lib9c.GraphQL.Types;

public class InventoryType : ObjectType<InventoryObject>
{
    protected override void Configure(IObjectTypeDescriptor<InventoryObject> descriptor)
    {
        descriptor
            .Field(f => f.Consumables)
            .Name("consumables")
            .Description("The consumables in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>();
        descriptor
            .Field(f => f.Costumes)
            .Name("costumes")
            .Description("The costumes in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>();
        descriptor
            .Field(f => f.Equipments)
            .Name("equipments")
            .Description("The equipments in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>();
        descriptor
            .Field(f => f.Materials)
            .Name("materials")
            .Description("The materials in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>();
    }
}
