using Lib9c.GraphQL.Objects;
using Mimir.Models.Avatar;

namespace Mimir.GraphQL.Factories;

public static class InventoryObjectFactory
{
    public static InventoryObject Create(Inventory inventory)
    {
        var consumables = inventory.Consumables.Select(ItemObjectFactory.Create).ToArray();
        var costumes = inventory.Costumes.Select(ItemObjectFactory.Create).ToArray();
        var equipments = inventory.Equipments.Select(ItemObjectFactory.Create).ToArray();
        var materials = inventory.Materials.Select(ItemObjectFactory.Create).ToArray();
        return new InventoryObject(
            consumables,
            costumes,
            equipments,
            materials);
    }
}
