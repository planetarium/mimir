using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;

namespace Mimir.GraphQL.Resolvers;

public class InventoryResolver
{
    public static Inventory GetInventory(
        IResolverContext context,
        [Service] InventoryRepository inventoryRepo,
        [Parent] InventoryObject inventoryObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("inventory")] Inventory? inventory)
    {
        if (inventory is not null)
        {
            return inventory;
        }

        var avatarAddress = inventoryObject.Address;
        inventory = inventoryRepo.GetInventory(planetName, avatarAddress);
        context.ScopedContextData = context.ScopedContextData.Add("inventory", inventory);
        return inventory;
    }

    public static ItemObject[] GetConsumables(
        IResolverContext context,
        [Service] InventoryRepository inventoryRepo,
        [Parent] InventoryObject inventoryObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("inventory")] Inventory? inventory) =>
        GetInventory(context, inventoryRepo, inventoryObject, planetName, inventory)
            .Consumables
            .Select(ItemObjectFactory.Create)
            .ToArray();

    public static ItemObject[] GetCostumes(
        IResolverContext context,
        [Service] InventoryRepository inventoryRepo,
        [Parent] InventoryObject inventoryObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("inventory")] Inventory? inventory) =>
        GetInventory(context, inventoryRepo, inventoryObject, planetName, inventory)
            .Costumes
            .Select(ItemObjectFactory.Create)
            .ToArray();

    public static ItemObject[] GetEquipments(
        IResolverContext context,
        [Service] InventoryRepository inventoryRepo,
        [Parent] InventoryObject inventoryObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("inventory")] Inventory? inventory) =>
        GetInventory(context, inventoryRepo, inventoryObject, planetName, inventory)
            .Equipments
            .Select(ItemObjectFactory.Create)
            .ToArray();

    public static ItemObject[] GetMaterials(
        IResolverContext context,
        [Service] InventoryRepository inventoryRepo,
        [Parent] InventoryObject inventoryObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("inventory")] Inventory? inventory) =>
        GetInventory(context, inventoryRepo, inventoryObject, planetName, inventory)
            .Materials
            .Select(ItemObjectFactory.Create)
            .ToArray();
}
