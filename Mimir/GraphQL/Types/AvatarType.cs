using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class AvatarType : ObjectType<AvatarObject>
{
    protected override void Configure(IObjectTypeDescriptor<AvatarObject> descriptor)
    {
        descriptor
            .Field("inventory")
            .Type<InventoryType>()
            .Resolve(context =>
            {
                var avatarRepo = context.Services.GetService<AvatarRepository>();
                if (avatarRepo is null)
                {
                    return null;
                }

                var planetName = context.ScopedContextData.TryGetValue("planetName", out var pn)
                    ? (PlanetName?)pn
                    : null;
                if (planetName is null)
                {
                    return null;
                }
                
                var avatarAddress = context.ScopedContextData.TryGetValue("avatarAddress", out var aa)
                    ? (Address?)aa
                    : null;
                if (avatarAddress is null)
                {
                    return null;
                }
                
                var inventory = avatarRepo.GetInventory(planetName.Value.ToString(), avatarAddress.Value);
                if (inventory is null)
                {
                    return null;
                }
                
                context.ScopedContextData = context.ScopedContextData.Add("inventory", inventory);
                return new InventoryObject();
            });
    }
}
