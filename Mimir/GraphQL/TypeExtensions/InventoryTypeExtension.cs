
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types;
using Mimir.Repositories;

namespace Mimir.GraphQL.TypeExtensions;

public class InventoryTypeExtension : ObjectTypeExtension<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("inventory")
            .Argument("planetName", a => a.Type<NonNullType<PlanetNameEnumType>>())
            .Argument("avatarAddress", a => a.Type<NonNullType<AddressType>>())
            .Type<InventoryType>()
            .ResolveWith<InventoryTypeExtension>(_ => GetInventory(
                default!,
                default!,
                default!));
    }

    private static InventoryObject? GetInventory(
        string planetName,
        Address avatarAddress,
        [Service] AvatarRepository avatarRepository)
    {
        var inventory = avatarRepository.GetInventory(planetName, avatarAddress);
        if (inventory is null)
        {
            return null;
        }

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
