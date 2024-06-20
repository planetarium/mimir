using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Queries;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("agent")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AgentType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new AgentObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("avatar")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AvatarType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new AvatarObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("inventory")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<InventoryType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new InventoryObject(context.ArgumentValue<Address>("address"));
            });
    }
}
