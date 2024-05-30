using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models.Avatar;

namespace Mimir.GraphQL.Types;

public class InventoryType : ObjectType<InventoryObject>
{
    protected override void Configure(IObjectTypeDescriptor<InventoryObject> descriptor)
    {
        descriptor
            .Field("consumables")
            .Description("The consumables in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .Resolve(context => context.ScopedContextData.TryGetValue("inventory", out var value)
                ? ((Inventory)value!).Consumables.Select(ItemObjectFactory.Create).ToArray()
                : []);
        descriptor
            .Field("costumes")
            .Description("The costumes in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .Resolve(context => context.ScopedContextData.TryGetValue("inventory", out var value)
                ? ((Inventory)value!).Costumes.Select(ItemObjectFactory.Create).ToArray()
                : []);
        descriptor
            .Field("equipments")
            .Description("The equipments in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .Resolve(context => context.ScopedContextData.TryGetValue("inventory", out var value)
                ? ((Inventory)value!).Equipments.Select(ItemObjectFactory.Create).ToArray()
                : []);
        descriptor
            .Field("materials")
            .Description("The materials in the inventory.")
            .Type<NonNullType<ListType<NonNullType<ItemType>>>>()
            .Resolve(context => context.ScopedContextData.TryGetValue("inventory", out var value)
                ? ((Inventory)value!).Materials.Select(ItemObjectFactory.Create).ToArray()
                : []);
    }
}
